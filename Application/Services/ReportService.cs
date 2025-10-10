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

namespace Application.Services;

public class ReportService(EntityContext context, IFileService fileService, IHttpContextAccessor httpContextAccessor) : IReport
{
    public async Task<Result<ReportViewModel>> CreateReport(CreateReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify patient exists
            var patient = await context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId && p.Status != EntityStatus.Deleted, cancellationToken);
                
            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            // Verify doctor exists
            var doctor = await context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.Status != EntityStatus.Deleted, cancellationToken);
                
            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            // Verify appointment if provided
            if (request.AppointmentId.HasValue)
            {
                var appointment = await context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == request.AppointmentId.Value && 
                                            a.PatientId == request.PatientId && 
                                            a.DoctorId == request.DoctorId, cancellationToken);
                                            
                if (appointment == null)
                    return new ErrorModel(ErrorEnum.AppointmentNotFound);
            }

            string? pdfPath = null;
            
            // Handle file upload if provided
            if (request.ReportFile != null)
            {
                var fileResult = await fileService.SaveFileAsync(request.ReportFile, "reports");
                if (!fileResult.Success)
                    return fileResult.Error!;
                    
                pdfPath = fileResult.Payload;
            }

            // Create report
            var report = new Report
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ReportText = request.ReportText,
                AppointmentId = request.AppointmentId,
                PdfPath = pdfPath,
                Notes = request.Notes,
                ReportDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await context.Reports.AddAsync(report, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Load with includes for view model
            var savedReport = await context.Reports
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Appointment)
                .FirstAsync(r => r.Id == report.Id, cancellationToken);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new ReportViewModel(savedReport, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<ReportViewModel>> GetReportById(long reportId, CancellationToken cancellationToken)
    {
        try
        {
            var report = await context.Reports
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Appointment)
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted, cancellationToken);
                
            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new ReportViewModel(report, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<ReportViewModel>>> GetReports(FilterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = context.Reports
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Appointment)
                .Where(r => r.Status != EntityStatus.Deleted);

            // Apply filters
            if (request.PatientId.HasValue)
                query = query.Where(r => r.PatientId == request.PatientId.Value);

            if (request.DoctorId.HasValue)
                query = query.Where(r => r.DoctorId == request.DoctorId.Value);

            if (request.FromDate.HasValue)
                query = query.Where(r => r.ReportDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(r => r.ReportDate <= request.ToDate.Value);

            if (!string.IsNullOrEmpty(request.SearchText))
                query = query.Where(r => r.ReportText.Contains(request.SearchText) || 
                                       r.Notes!.Contains(request.SearchText));

            var reports = await query
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);

            var baseUrl = httpContextAccessor.GetRequestPath();
            var reportViewModels = reports.Select(r => new ReportViewModel(r, baseUrl)).ToList();

            return request.All ? 
                reportViewModels.ToListResponse() : 
                reportViewModels.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> DownloadReportPdf(long reportId, CancellationToken cancellationToken)
    {
        try
        {
            var report = await context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted, cancellationToken);
                
            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            if (string.IsNullOrEmpty(report.PdfPath))
                return new ErrorModel(ErrorEnum.FileNotFound);

            return await fileService.GetFileAsync(report.PdfPath);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteReport(long reportId, CancellationToken cancellationToken)
    {
        try
        {
            var report = await context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.Status != EntityStatus.Deleted, cancellationToken);
                
            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            // Soft delete from database
            report.Status = EntityStatus.Deleted;
            report.UpdatedDate = DateTime.UtcNow;
            
            await context.SaveChangesAsync(cancellationToken);

            // Delete physical file if exists
            if (!string.IsNullOrEmpty(report.PdfPath))
            {
                await fileService.DeleteFileAsync(report.PdfPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<ReportViewModel>> UploadReportFile(UploadReportFileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await context.Reports
                .Include(r => r.Patient)
                    .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.Id == request.ReportId && r.Status != EntityStatus.Deleted, cancellationToken);
                
            if (report == null)
                return new ErrorModel(ErrorEnum.ReportNotFound);

            // Delete existing file if any
            if (!string.IsNullOrEmpty(report.PdfPath))
            {
                await fileService.DeleteFileAsync(report.PdfPath);
            }

            // Save new file
            var fileResult = await fileService.SaveFileAsync(request.File, "reports");
            if (!fileResult.Success)
                return fileResult.Error!;

            // Update report with new file path
            report.PdfPath = fileResult.Payload;
            report.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            var baseUrl = httpContextAccessor.GetRequestPath();
            return new ReportViewModel(report, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}