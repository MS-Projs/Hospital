using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface ICertificateType
{
    Task<Result<CertificateTypeViewModel>> UpsertCertificateType(UpsertCertificateTypeRequest contractTypeRequest,
        long currentUserId);

    Task<Result<CertificateTypeViewModel>> GetType(long certificateTypeId, long currentUserId);
    Task<Result<PagedResult<CertificateTypeViewModel>>> GetTypes(PagedRequest request, long currentUserId);

    Task<Result<CertificateTypeViewModel>> ToggleCertificateTypeActivation(long certificateTypeId,
        long currentUserId);
}   