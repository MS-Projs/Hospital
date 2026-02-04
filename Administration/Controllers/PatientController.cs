using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

/// <summary>
/// Controller for patient management operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]
public class PatientController : MyController<PatientController>
{
    private readonly IPatient _patientService;

    public PatientController(IPatient patientService)
    {
        _patientService = patientService;
    }

    #region CRUD Operations

    /// <summary>
    /// Create or update a patient
    /// </summary>
    /// <param name="request">Patient information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Patient view model</returns>
    [HttpPost]
    public async Task<Result<PatientViewModel>> UpsertPatient(
        UpsertPatientRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _patientService.UpsertPatient(request, cancellationToken);
    }

    /// <summary>
    /// Get patient by ID
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Patient single view model</returns>
    [HttpGet]
    public async Task<Result<PatientSingleViewModel>> GetPatientById(
        [FromQuery] long patientId, 
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatientById(patientId, cancellationToken);
    }

    /// <summary>
    /// Get paginated list of patients with optional filtering
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of patients</returns>
    [HttpPost]
    public async Task<Result<PagedResult<PatientViewModel>>> GetPatients(
        FilterPatientRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatients(request, cancellationToken);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle patient activation status (activate/deactivate)
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated patient view model</returns>
    [HttpPost]
    public async Task<Result<PatientViewModel>> ToggleActivation(
        [FromQuery] long patientId, 
        CancellationToken cancellationToken = default)
    {
        return await _patientService.PatientToggleActivation(patientId, cancellationToken);
    }

    #endregion

    #region Patient Document Management

    /// <summary>
    /// Upload a document for a patient
    /// </summary>
    /// <param name="request">Document upload request containing patient ID, file, and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Uploaded document details</returns>
    [HttpPost("upload-document")]
    public async Task<Result<DocumentViewModel>> UploadDocument(
        [FromForm] UploadPatientDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.UploadPatientDocument(request, cancellationToken);
    }

    /// <summary>
    /// Get all documents for a specific patient
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of patient documents</returns>
    [HttpGet("documents")]
    public async Task<Result<List<DocumentViewModel>>> GetDocuments(
        [FromQuery] long patientId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.GetPatientDocuments(patientId,UserId, cancellationToken);
    }

    /// <summary>
    /// Download a specific patient document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document file stream</returns>
    [HttpGet("download-document")]
    public async Task<IActionResult> DownloadDocument(
        [FromQuery] long documentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientService.DownloadPatientDocument(documentId, UserId,cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        var (stream, fileName, contentType) = result.Payload!;
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Delete a patient document (soft delete)
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("document")]
    public async Task<Result<bool>> DeleteDocument(
        [FromQuery] long documentId,
        CancellationToken cancellationToken = default)
    {
        return await _patientService.DeletePatientDocument(documentId, cancellationToken);
    }

    #endregion
}
