using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
[Authorize] 
public class PatientController : MyController<PatientController>
{
    private readonly IPatient _patientService;

    public PatientController(IPatient patientService)
    {
        _patientService = patientService;
    }

    #region CRUD Operations

    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<Result<PatientViewModel>> UpsertPatient(
        UpsertPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.UpsertPatient(request, cancellationToken);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<Result<PatientSingleViewModel>> GetPatientById(
        [FromQuery] long patientId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatientById(patientId, cancellationToken);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Result<PagedResult<PatientViewModel>>> GetPatients(
        FilterPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatients(request, cancellationToken);
    }

    #endregion

    #region Status Management

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Result<PatientViewModel>> ToggleActivation(
        [FromQuery] long patientId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.PatientToggleActivation(patientId, cancellationToken);
    }

    #endregion

    #region Patient Document Management

    [HttpPost("upload-document")]
    [Authorize(Roles = "Doctor")]
    public async Task<Result<DocumentViewModel>> UploadDocument(
        [FromForm] UploadPatientDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.UploadPatientDocument(request, cancellationToken);
    }

    [HttpGet("documents")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<Result<List<DocumentViewModel>>> GetDocuments(
        [FromQuery] long patientId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatientDocuments(patientId, UserId, cancellationToken);
    }

    [HttpGet("download-document")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> DownloadDocument(
        [FromQuery] long documentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientService.DownloadPatientDocument(documentId, UserId, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        var (stream, fileName, contentType) = result.Payload!;
        return File(stream, contentType, fileName);
    }

    [HttpDelete("document")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<Result<bool>> DeleteDocument(
        [FromQuery] long documentId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.DeletePatientDocument(documentId, cancellationToken);
    }

    #endregion
}