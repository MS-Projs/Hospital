using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

/// <summary>
/// Controller for doctor management operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]
public class DoctorController : MyController<DoctorController>
{
    private readonly IDoctor _doctorService;

    public DoctorController(IDoctor doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Create or update a doctor
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async Task<Result<DoctorViewModel>> UpsertDoctor(
        UpsertDoctorRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.UpsertDoctor(request, cancellationToken);
    }

    /// <summary>
    /// Get doctor by ID
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<Result<DoctorSingleViewModel>> GetDoctorById(
        [FromQuery] long doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctorById(doctorId, cancellationToken);
    }

    /// <summary>
    /// Get paginated list of doctors with optional filtering
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(
        FilterDoctorRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctors(request, cancellationToken);
    }

    /// <summary>
    /// Toggle doctor activation status - Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Result<DoctorViewModel>> ToggleActivation(
        [FromQuery] long doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.DoctorToggleActivation(doctorId, cancellationToken);
    }

    /// <summary>
    /// Upload a certificate for a doctor
    /// </summary>
    [HttpPost("upload-certificate")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<Result<CertificateViewModel>> UploadCertificate(
        [FromForm] UploadDoctorCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.UploadDoctorCertificate(request, cancellationToken);
    }

    /// <summary>
    /// Get all certificates for a specific doctor
    /// </summary>
    [HttpGet("certificates")]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<Result<List<CertificateViewModel>>> GetCertificates(
        [FromQuery] long doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctorCertificates(doctorId, cancellationToken);
    }

    /// <summary>
    /// Download a specific doctor certificate
    /// </summary>
    [HttpGet("download-certificate")]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<IActionResult> DownloadCertificate(
        [FromQuery] long certificateId,
        CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.DownloadDoctorCertificate(certificateId, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        var (stream, fileName, contentType) = result.Payload!;
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Delete a doctor certificate - Admin and Doctor only
    /// </summary>
    [HttpDelete("certificate")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<Result<bool>> DeleteCertificate(
        [FromQuery] long certificateId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.DeleteDoctorCertificate(certificateId, cancellationToken);
    }
}