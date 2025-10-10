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

public class PatientService : IPatient
{
    private readonly EntityContext _context;

    public PatientService(EntityContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientViewModel>> UpsertPatient(UpsertPatientRequest patientRequest, CancellationToken cancellationToken)
    {
        try
        {
            patientRequest.FullName = patientRequest.FullName.Trim();
            
            PatientViewModel result;

            if (patientRequest.Id == 0)
            {
                var userIsExist = await _context.Patients.FirstOrDefaultAsync(x =>
                    x.UserId == patientRequest.UserId, cancellationToken: cancellationToken);

                if (userIsExist != null) return new ErrorModel(ErrorEnum.PatientAlreadyExist);

                result = await InsertPatient(patientRequest,cancellationToken);
            }
            else  
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(c => c.Id == patientRequest.Id && 
                                              c.Status != EntityStatus.Deleted, cancellationToken: cancellationToken);

                if (patient == null) return new ErrorModel(ErrorEnum.PatientNotFound);

                result = await UpdatePatient(patientRequest,cancellationToken);
            }
           
            return result;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    private async Task<PatientViewModel> UpdatePatient(UpsertPatientRequest patientRequest, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(c => c.Id == patientRequest.Id, cancellationToken: cancellationToken);

        patient!.UserId = patientRequest.UserId;
        patient.FullName = patientRequest.FullName.Trim();
        patient.AdditionalNotes = patientRequest.AdditionalNotes!.Trim();
        patient.Age = patientRequest.Age;
        patient.Gender = patientRequest.Gender.ToLower();
        patient.Address = patientRequest.Address;
        patient.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new PatientViewModel(patient);
    }

    private async Task<PatientViewModel> InsertPatient(UpsertPatientRequest patientRequest, CancellationToken cancellationToken)
    {
        var patient = patientRequest.Adapt<Patient>();
        patient.CreatedDate = DateTime.UtcNow;
        await _context.Patients.AddAsync(patient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new PatientViewModel(patient);
    }

    public async Task<Result<PatientSingleViewModel>> GetPatientById(long patientId, CancellationToken cancellationToken)
    {
        try
        {
            
            var patient = await _context.Patients.AsQueryable()
                .FirstOrDefaultAsync(d => d.Id == patientId && d.Status != EntityStatus.Deleted, cancellationToken: cancellationToken);

            if (patient is null)
                return new ErrorModel(ErrorEnum.PatientNotFound);
            
            return new PatientSingleViewModel(patient);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<PatientViewModel>>> GetPatients(FilterPatientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.FullName = request.FullName?.Trim();
            
            var patients = _context.Patients.AsQueryable();
            
            patients = patients.Where(x => x.Status != EntityStatus.Deleted);

            if (request?.FullName?.Length > 0)
                patients = patients.Where(x =>
                    x.FullName.Contains(request.FullName));

            if (request?.Gender?.Length > 0)
                patients = patients.Where(x =>
                    x.Gender.Contains(request.Gender));
            
            if ( request?.Age != 0) 
                patients = patients.Where(x => x.Age >= request!.Age);

                      
            var result = await patients.OrderBy(x => x.FullName)
                .Select(x => new PatientViewModel(x))
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

    public async Task<Result<PatientViewModel>> PatientToggleActivation(long patientId, CancellationToken cancellationToken)
    {
        try {
            
            var patient = await _context.Patients
                .FirstOrDefaultAsync(c => c.Id == patientId, cancellationToken: cancellationToken);

            if (patient is null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            patient.Status = EntityStatus.Deleted;
            patient.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            

            return new PatientViewModel(patient);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }                
    }
}