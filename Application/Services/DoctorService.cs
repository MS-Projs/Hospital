using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DoctorService : IDoctor
{
    private readonly EntityContext _context;

    public DoctorService(EntityContext context)
    {
        _context = context;
    }

    public async Task<Result<DoctorViewModel>> UpsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken)
    {
        try
        {
            doctorRequest.FullName = doctorRequest.FullName.Trim();
            
            DoctorViewModel result;

            if (doctorRequest.Id == 0)
            {
                var userIsExist = await _context.Doctors.FirstOrDefaultAsync(x =>
                    x.UserId == doctorRequest.UserId, cancellationToken: cancellationToken);

                if (userIsExist != null) return new ErrorModel(ErrorEnum.DoctorAlreadyExist);

                result = await InsertDoctor(doctorRequest,cancellationToken);
            }
            else  
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(c => c.Id == doctorRequest.Id && 
                                              c.Status != EntityStatus.Deleted, cancellationToken: cancellationToken);

                if (doctor == null) return new ErrorModel(ErrorEnum.DoctorNotFound);

                result = await UpdateDoctor(doctorRequest,cancellationToken);
            }
           
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
        doctor.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        return new DoctorViewModel(doctor);
    }

    private async Task<DoctorViewModel> InsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken)
    {
        var doctor = doctorRequest.Adapt<Doctor>();
        doctor.CreatedDate = DateTime.Now;
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
            
            if ( request?.ExperienceYears != 0) 
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
            doctor.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);

            return new DoctorViewModel(doctor);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}