using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IDoctor
{
    Task<Result<DoctorViewModel>> UpsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken);
    Task<Result<DoctorSingleViewModel>> GetDoctorById(long doctorId, CancellationToken cancellationToken);

    Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(FilterDoctorRequest request,
        CancellationToken cancellationToken);

    Task<Result<DoctorViewModel>> DoctorToggleActivation(long doctorId,
        CancellationToken cancellationToken);
}