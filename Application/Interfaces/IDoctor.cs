using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IDoctor
{
    Task<Result<DoctorViewModel>> UpsertDoctor(UpsertDoctorRequest doctorRequest, CancellationToken cancellationToken);
    Task<Result<DoctorSingleViewModel>> GetDoctorById(long doctorId, CancellationToken cancellationToken);

    Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(FilterDoctorRequest request, CancellationToken cancellationToken);

    Task<Result<DoctorViewModel>> DoctorToggleActivation(long doctorId, CancellationToken cancellationToken);

    Task<Result<DoctorViewModel>> ApproveDoctor(long doctorId, CancellationToken cancellationToken);

    Task<Result<bool>> RejectDoctor(long doctorId, string? reason, CancellationToken cancellationToken);

    Task<Result<PagedResult<DoctorViewModel>>> GetPendingDoctors(PagedRequest request, CancellationToken cancellationToken);
    
    // Certificate Management
    Task<Result<CertificateViewModel>> UploadDoctorCertificate(UploadDoctorCertificateRequest request, CancellationToken cancellationToken);
    Task<Result<List<CertificateViewModel>>> GetDoctorCertificates(long doctorId, CancellationToken cancellationToken);
    Task<Result<(Stream stream, string fileName, string contentType)>> DownloadDoctorCertificate(long certificateId, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteDoctorCertificate(long certificateId, CancellationToken cancellationToken);
}