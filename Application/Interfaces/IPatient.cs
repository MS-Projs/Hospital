using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IPatient
{
    Task<Result<PatientViewModel>> UpsertPatient(UpsertPatientRequest patientRequest,
        CancellationToken cancellationToken);

    Task<Result<PatientSingleViewModel>> GetPatientById(long patientId, CancellationToken cancellationToken);

    Task<Result<PagedResult<PatientViewModel>>> GetPatients(FilterPatientRequest request,
        CancellationToken cancellationToken);

    Task<Result<PatientViewModel>> PatientToggleActivation(long patientId,
        CancellationToken cancellationToken);

    // Document Management
    Task<Result<DocumentViewModel>> UploadPatientDocument(UploadPatientDocumentRequest request,
        CancellationToken cancellationToken);

    Task<Result<List<DocumentViewModel>>> GetPatientDocuments(long patientId, CancellationToken cancellationToken);

    Task<Result<(Stream stream, string fileName, string contentType)>> DownloadPatientDocument(long documentId,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeletePatientDocument(long documentId, CancellationToken cancellationToken);
}