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

namespace Application.Services;

public class DoctorService : IDoctor
{
    private readonly EntityContext _context;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DoctorService(EntityContext context, IFileService fileService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<DoctorViewModel>> UpsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken)
    {
        try
        {
            doctorRequest.FullName = doctorRequest.FullName.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == doctorRequest.UserId && u.Status != EntityStatus.Deleted,
                    cancellationToken: cancellationToken);
            if(user==null)
                return new ErrorModel(ErrorEnum.UserNotFound);
            DoctorViewModel result;

            if (doctorRequest.Id == 0)
            {
                var userIsExist = await _context.Doctors.FirstOrDefaultAsync(x =>
                    x.UserId == doctorRequest.UserId, cancellationToken: cancellationToken);

                if (userIsExist != null) return new ErrorModel(ErrorEnum.DoctorAlreadyExist);

                result = await InsertDoctor(doctorRequest,cancellationToken);
                user.Role = Role.Doctor;
            }
            else  
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(c => c.Id == doctorRequest.Id && 
                                              c.Status != EntityStatus.Deleted, cancellationToken: cancellationToken);

                if (doctor == null) return new ErrorModel(ErrorEnum.DoctorNotFound);

                result = await UpdateDoctor(doctorRequest,cancellationToken);
                user.Role = Role.Doctor;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    private async Task<DoctorViewModel> UpdateDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(c => c.Id == doctorRequest.Id, cancellationToken: cancellationToken);

        doctor!.UserId = doctorRequest.UserId;
        doctor.FullName = doctorRequest.FullName.Trim();
        doctor.Specialization = doctorRequest.Specialization.Trim().ToLower();
        doctor.ExperienceYears = doctorRequest.ExperienceYears;
        doctor.Workplace = doctorRequest.Workplace;
        doctor.Biography = doctorRequest.Biography;
        doctor.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DoctorViewModel(doctor);
    }

    private async Task<DoctorViewModel> InsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken)
    {
        var doctor = doctorRequest.Adapt<Doctor>();
        doctor.CreatedDate = DateTime.UtcNow;
        await _context.Doctors.AddAsync(doctor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        
        return new DoctorViewModel(doctor);
    }

    public async Task<Result<DoctorSingleViewModel>> GetDoctorById(long doctorId, CancellationToken cancellationToken)
    {
        try
        {
            
            var doctor = await _context.Doctors.AsQueryable()
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted);

            if (doctor is null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);
            
            return new DoctorSingleViewModel(doctor);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(FilterDoctorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            
            request.FullName = request.FullName?.Trim();
            
            var doctors = _context.Doctors.AsQueryable();
            
            doctors = doctors.Where(x => x.Status != EntityStatus.Deleted);

            if (request?.FullName?.Length > 0)
                doctors = doctors.Where(x =>
                    x.FullName.Contains(request.FullName));

            if (request?.Specialization?.Length > 0)
                doctors = doctors.Where(x =>
                    x.Specialization.Contains(request.Specialization));
            
            if ( request?.ExperienceYears != null) 
                doctors = doctors.Where(x => x.ExperienceYears >= request!.ExperienceYears);

            
            var result = await doctors.OrderBy(x => x.FullName)
                .Select(x => new DoctorViewModel(x))
                .ToListAsync(cancellationToken: cancellationToken);
          
            return request.All ? 
                result.ToListResponse() : 
                result.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<DoctorViewModel>> DoctorToggleActivation(long doctorId, CancellationToken cancellationToken)
    {
        try {
            
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(c => c.Id == doctorId, cancellationToken: cancellationToken);

            if (doctor is null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            doctor.Status = EntityStatus.Deleted;
            doctor.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new DoctorViewModel(doctor);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    // Certificate Management Methods
    public async Task<Result<CertificateViewModel>> UploadDoctorCertificate(UploadDoctorCertificateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify doctor exists
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.Status != EntityStatus.Deleted, cancellationToken);
                
            if (doctor == null)
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
                FileType = request.FileType,
                CertificateTypeId = request.CategoryId,
                EncryptedKey = _fileService.GenerateEncryptionKey(),
                UploadedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            await _context.DoctorCertificates.AddAsync(certificate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load with includes for view model
            var savedCertificate = await _context.DoctorCertificates
                .Include(c => c.CertificateType)
                .FirstAsync(c => c.Id == certificate.Id, cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            return new CertificateViewModel(savedCertificate, baseUrl);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<List<CertificateViewModel>>> GetDoctorCertificates(long doctorId, CancellationToken cancellationToken)
    {
        try
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted, cancellationToken);
                
            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            var certificates = await _context.DoctorCertificates
                .Include(c => c.CertificateType)
                .Where(c => c.DoctorId == doctorId && c.Status != EntityStatus.Deleted)
                .OrderByDescending(c => c.UploadedAt)
                .ToListAsync(cancellationToken);

            var baseUrl = _httpContextAccessor.GetRequestPath();
            var certificateViewModels = certificates.Select(c => new CertificateViewModel(c, baseUrl)).ToList();

            return certificateViewModels;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> DownloadDoctorCertificate(long certificateId, CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _context.DoctorCertificates
                .FirstOrDefaultAsync(c => c.Id == certificateId && c.Status != EntityStatus.Deleted, cancellationToken);
                
            if (certificate == null)
                return new ErrorModel(ErrorEnum.CertificateNotFound);

            return await _fileService.GetFileAsync(certificate.FilePath);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteDoctorCertificate(long certificateId, CancellationToken cancellationToken)
    {
        try
        {
            var certificate = await _context.DoctorCertificates
                .FirstOrDefaultAsync(c => c.Id == certificateId && c.Status != EntityStatus.Deleted, cancellationToken);
                
            if (certificate == null)
                return new ErrorModel(ErrorEnum.CertificateNotFound);

            // Soft delete from database
            certificate.Status = EntityStatus.Deleted;
            certificate.UpdatedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);

            // Delete physical file (optional - you might want to keep files for audit trail)
            await _fileService.DeleteFileAsync(certificate.FilePath);

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}