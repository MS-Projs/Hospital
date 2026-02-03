using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Infrastructure.Extensions;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ReportService : IReport
{
    private readonly EntityContext _context;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        EntityContext context,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReportService> logger)
    {
        _context = context;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<ReportViewModel>> CreateReport(
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate patient and doctor in parallel
            var (patient, doctor) = await ValidatePatientAndDoctor(
                request.PatientId,
                request.DoctorId,
                cancellationToken);

            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            // Validate appointment if provided
            if (request.AppointmentId.HasValue)
            {
                var appointmentValid = await ValidateAppointment(
                    request.AppointmentId.Value,
                    request.PatientId,
                    request.DoctorId,
                    cancellationToken);

                if (!appointmentValid)
                    return new ErrorModel(ErrorEnum.AppointmentNotFound);
            }

            string? pdfPath = null;

            // Handle file upload
            if (request.ReportFile != null)
            {
                var fileResult = await _fileService.SaveFileAsync(request.ReportFile, "reports");
                if (!fileResult.Success)
                    return fileResult.Error!;

                pdfPath = fileResult.Payload;
            }

            // Create report
            var report = new Report
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ReportText = request.ReportText?.Trim() ?? string.Empty,
                AppointmentId = request.AppointmentId,
                PdfPath = pdfPath,
                Notes = request.Notes?.Trim(),
                ReportDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.Reports.AddAsync(report, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load with includes
            var savedReport = await GetReportWithIncludes(report.Id, cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new ReportViewModel(savedReport!, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report for patient {PatientId} and doctor {DoctorId}",
                request.PatientId, request.DoctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<ReportViewModel>> GetReportById(
        long reportId,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await GetReportWithIncludes(reportId, cancellationToken);

            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new ReportViewModel(report, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report {ReportId}", reportId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<ReportViewModel>>> GetReports(
        FilterReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = BuildReportQuery(request);

            var reports = await query
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            var reportViewModels = reports
                .Select(r => new ReportViewModel(r, baseUrl))
                .ToList();

            return request.All
                ? reportViewModels.ToListResponse()
                : reportViewModels.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports with filter");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> DownloadReportPdf(
        long reportId,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _context.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            if (string.IsNullOrEmpty(report.PdfPath))
                return new ErrorModel(ErrorEnum.FileNotFound);

            return await _fileService.GetFileAsync(report.PdfPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading report PDF {ReportId}", reportId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteReport(
        long reportId,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            // Soft delete
            report.Status = EntityStatus.Deleted;
            report.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete physical file if exists (background operation)
            if (!string.IsNullOrEmpty(report.PdfPath))
            {
                _ = Task.Run(() => _fileService.DeleteFileAsync(report.PdfPath), CancellationToken.None);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId}", reportId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<ReportViewModel>> UploadReportFile(
        UploadReportFileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await GetReportWithIncludes(request.ReportId, cancellationToken);

            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            // Delete existing file if any
            if (!string.IsNullOrEmpty(report.PdfPath))
            {
                await _fileService.DeleteFileAsync(report.PdfPath);
            }

            // Save new file
            var fileResult = await _fileService.SaveFileAsync(request.File, "reports");
            if (!fileResult.Success)
                return fileResult.Error!;

            // Update report
            report.PdfPath = fileResult.Payload;
            report.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new ReportViewModel(report, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for report {ReportId}", request.ReportId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #region Private Helper Methods

    private async Task<(Patient? patient, Doctor? doctor)> ValidatePatientAndDoctor(
        long patientId,
        long doctorId,
        CancellationToken cancellationToken)
    {
        var patientTask = _context.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId && p.Status != EntityStatus.Deleted,
                cancellationToken);

        var doctorTask = _context.Doctors
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted,
                cancellationToken);

        await Task.WhenAll(patientTask, doctorTask);

        return (await patientTask, await doctorTask);
    }

    private async Task<bool> ValidateAppointment(
        long appointmentId,
        long patientId,
        long doctorId,
        CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.Id == appointmentId &&
                a.PatientId == patientId &&
                a.DoctorId == doctorId &&
                a.Status != EntityStatus.Deleted,
                cancellationToken);
    }

    private async Task<Report?> GetReportWithIncludes(
        long reportId,
        CancellationToken cancellationToken)
    {
        return await _context.Reports
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
            .Include(r => r.Appointment)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted,
                cancellationToken);
    }

    private IQueryable<Report> BuildReportQuery(FilterReportRequest request)
    {
        var query = _context.Reports
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)
            .Include(r => r.Appointment)
            .Where(r => r.Status != EntityStatus.Deleted)
            .AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(r => r.PatientId == request.PatientId.Value);

        if (request.DoctorId.HasValue)
            query = query.Where(r => r.DoctorId == request.DoctorId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.ReportDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.ReportDate <= request.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();
            query = query.Where(r =>
                EF.Functions.ILike(r.ReportText, $"%{searchText}%") ||
                (r.Notes != null && EF.Functions.ILike(r.Notes, $"%{searchText}%")));
        }

        return query;
    }

    #endregion
}