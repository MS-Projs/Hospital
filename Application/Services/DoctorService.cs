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
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DoctorService : IDoctor
{
    private readonly EntityContext _context;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        EntityContext context,
        IFileService fileService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DoctorService> logger)
    {
        _context = context;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    #region UpsertDoctor

    public async Task<Result<DoctorViewModel>> UpsertDoctor(
        UpsertDoctorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Normalize input
            request.FullName = request.FullName?.Trim() ?? string.Empty;
            request.Specialization = request.Specialization?.Trim().ToLowerInvariant() ?? string.Empty;
            request.Workplace = request.Workplace?.Trim();
            request.Biography = request.Biography?.Trim();

            // Validate user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (user == null)
                return new ErrorModel(ErrorEnum.UserNotFound);

            DoctorViewModel result;

            if (request.Id == 0)
            {
                // Check for existing doctor
                var existingDoctor = await _context.Doctors
                    .AsNoTracking()
                    .AnyAsync(x => x.UserId == request.UserId,
                        cancellationToken);

                if (existingDoctor)
                    return new ErrorModel(ErrorEnum.DoctorAlreadyExist);

                result = await CreateDoctor(request, cancellationToken);

                // Update user role but keep inactive until admin approves
                user.Role = Role.Doctor;
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Verify doctor exists
                var doctorExists = await _context.Doctors
                    .AnyAsync(c => c.Id == request.Id && c.Status != EntityStatus.Deleted,
                        cancellationToken);

                if (!doctorExists)
                    return new ErrorModel(ErrorEnum.DoctorNotFound);

                result = await UpdateDoctor(request, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting doctor for user {UserId}", request.UserId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    private async Task<DoctorViewModel> CreateDoctor(
        UpsertDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = request.Adapt<Doctor>();
        doctor.CreatedDate = DateTime.UtcNow;
        doctor.Status = EntityStatus.Inactive;

        await _context.Doctors.AddAsync(doctor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await SyncDoctorRelations(doctor.Id, request, cancellationToken);

        _logger.LogInformation("New doctor {DoctorId} created with relations", doctor.Id);
        return new DoctorViewModel(doctor);
    }

    private async Task<DoctorViewModel> UpdateDoctor(
        UpsertDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .Include(d => d.Workplaces)
            .Include(d => d.Educations)
            .Include(d => d.Specializations)
            .FirstAsync(c => c.Id == request.Id, cancellationToken);

        request.Adapt(doctor);
        doctor.UpdatedDate = DateTime.UtcNow;

        await SyncDoctorRelations(doctor.Id, request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new DoctorViewModel(doctor);
    }

    private async Task SyncDoctorRelations(
        long doctorId,
        UpsertDoctorRequest request,
        CancellationToken cancellationToken)
    {
        // Workplaces
        _context.DoctorWorkplaces.RemoveRange(
            _context.DoctorWorkplaces.Where(x => x.DoctorId == doctorId));

        if (request.Workplaces != null)
        {
            await _context.DoctorWorkplaces.AddRangeAsync(
                request.Workplaces.Select(w => new DoctorWorkplace
                {
                    DoctorId = doctorId,
                    ClinicName = w.ClinicName,
                    ClinicAddress = w.ClinicAddress,
                    CreatedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                }), cancellationToken);
        }

        // Educations
        _context.DoctorEducations.RemoveRange(
            _context.DoctorEducations.Where(x => x.DoctorId == doctorId));

        if (request.Educations != null)
        {
            await _context.DoctorEducations.AddRangeAsync(
                request.Educations.Select(e => new DoctorEducation
                {
                    DoctorId = doctorId,
                    DegreeType = e.DegreeType,
                    University = e.University,
                    Country = e.Country,
                    GraduationYear = e.GraduationYear,
                    Specialization = e.Specialization,
                    CreatedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                }), cancellationToken);
        }

        // Specializations
        _context.DoctorSpecializations.RemoveRange(
            _context.DoctorSpecializations.Where(x => x.DoctorId == doctorId));

        if (request.Specializations != null)
        {
            await _context.DoctorSpecializations.AddRangeAsync(
                request.Specializations.Select(s => new DoctorSpecialization
                {
                    DoctorId = doctorId,
                    Name = s.Name,
                    CreatedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                }), cancellationToken);
        }
    }

    #endregion

    #region GetDoctor

    public async Task<Result<DoctorSingleViewModel>> GetDoctorById(
        long doctorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.Workplaces)
                .Include(d => d.Educations)
                .Include(d => d.Specializations)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            return new DoctorSingleViewModel(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor {DoctorId}", doctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(
        FilterDoctorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.FullName = request.FullName?.Trim();
            request.Specialization = request.Specialization?.Trim().ToLowerInvariant();

            var query = BuildDoctorQuery(request);

            var doctors = await query
                .OrderBy(x => x.FullName)
                .Select(x => new DoctorViewModel(x))
                .ToListAsync(cancellationToken);

            return request.All
                ? doctors.ToListResponse()
                : doctors.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors with filter");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    private IQueryable<Doctor> BuildDoctorQuery(FilterDoctorRequest request)
    {
        var query = _context.Doctors
            .Where(x => x.Status != EntityStatus.Deleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.FullName))
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{request.FullName}%"));

        if (!string.IsNullOrWhiteSpace(request.Specialization))
        {
            query = query.Where(d =>
                EF.Functions.ILike(d.Specialization, $"%{request.Specialization}%") ||
                d.Specializations.Any(s =>
                    EF.Functions.ILike(s.Name, $"%{request.Specialization}%")));
        }

        if (request.ExperienceYears.HasValue)
            query = query.Where(x => x.ExperienceYears >= request.ExperienceYears.Value);

        return query;
    }

    public async Task<Result<DoctorViewModel>> DoctorToggleActivation(
        long doctorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(c => c.Id == doctorId, cancellationToken);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            doctor.Status = doctor.Status == EntityStatus.Deleted
                ? EntityStatus.Active
                : EntityStatus.Deleted;
            doctor.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new DoctorViewModel(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling doctor {DoctorId} activation", doctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #endregion

    public async Task<Result<DoctorViewModel>> ApproveDoctor(
        long doctorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            doctor.Status = EntityStatus.Active;
            doctor.UpdatedDate = DateTime.UtcNow;

            if (doctor.User != null)
            {
                doctor.User.Status = EntityStatus.Active;
                doctor.User.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Doctor {DoctorId} approved", doctorId);

            return new DoctorViewModel(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving doctor {DoctorId}", doctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> RejectDoctor(
        long doctorId,
        string? reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            doctor.Status = EntityStatus.Deleted;
            doctor.UpdatedDate = DateTime.UtcNow;

            if (doctor.User != null)
            {
                doctor.User.Role = Role.User;
                doctor.User.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Doctor {DoctorId} rejected. Reason: {Reason}",
                doctorId, reason ?? "No reason");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting doctor {DoctorId}", doctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<DoctorViewModel>>> GetPendingDoctors(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Doctors
                .Where(d => d.Status == EntityStatus.Inactive);

            var doctors = await query
                .OrderByDescending(d => d.CreatedDate)
                .Select(d => new DoctorViewModel(d))
                .ToListAsync(cancellationToken);

            return request.All
                ? doctors.ToListResponse()
                : doctors.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending doctors");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #region Certificate Management

    public async Task<Result<CertificateViewModel>> UploadDoctorCertificate(
        UploadDoctorCertificateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify doctor exists
            var doctorExists = await _context.Doctors
                .AsNoTracking()
                .AnyAsync(d => d.Id == request.DoctorId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (!doctorExists)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            // Save file
            var fileResult = await _fileService.SaveFileAsync(request.File, "doctor-certificates");
            if (!fileResult.Success)
                return fileResult.Error!;

            // Create certificate record
            var certificate = new DoctorCertificate
            {
                DoctorId = request.DoctorId,
                FilePath = fileResult.Payload!,
                FileType = request.FileType?.Trim() ?? "Unknown",
                CertificateTypeId = request.CategoryId,
                EncryptedKey = _fileService.GenerateEncryptionKey(),
                UploadedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.DoctorCertificates.AddAsync(certificate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load with includes
            var savedCertificate = await _context.DoctorCertificates
                .Include(c => c.CertificateType)
                .AsNoTracking()
                .FirstAsync(c => c.Id == certificate.Id, cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new CertificateViewModel(savedCertificate, baseUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading certificate for doctor {DoctorId}", request.DoctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<List<CertificateViewModel>>> GetDoctorCertificates(
        long doctorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var doctorExists = await _context.Doctors
                .AsNoTracking()
                .AnyAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (!doctorExists)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            var certificates = await _context.DoctorCertificates
                .Include(c => c.CertificateType)
                .Where(c => c.DoctorId == doctorId && c.Status != EntityStatus.Deleted)
                .OrderByDescending(c => c.UploadedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            var certificateViewModels = certificates
                .Select(c => new CertificateViewModel(c, baseUrl))
                .ToList();

            return certificateViewModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificates for doctor {DoctorId}", doctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> DownloadDoctorCertificate(
        long certificateId,
        CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _context.DoctorCertificates
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == certificateId && c.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (certificate == null)
                return new ErrorModel(ErrorEnum.CertificateNotFound);

            return await _fileService.GetFileAsync(certificate.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading certificate {CertificateId}", certificateId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteDoctorCertificate(
        long certificateId,
        CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _context.DoctorCertificates
                .FirstOrDefaultAsync(c => c.Id == certificateId && c.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (certificate == null)
                return new ErrorModel(ErrorEnum.CertificateNotFound);

            // Soft delete
            certificate.Status = EntityStatus.Deleted;
            certificate.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete physical file (background operation)
            _ = Task.Run(() => _fileService.DeleteFileAsync(certificate.FilePath), CancellationToken.None);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting certificate {CertificateId}", certificateId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #endregion

    #region Certificates

    // Upload / Get / Download / Delete methods stay the same (old code compatible)

    #endregion
}