using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

/// <summary>
/// Controller for certificate type management operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]

public class CertificateTypeController : MyController<CertificateTypeController>
{
    private readonly ICertificateType _certificateTypeService;

    public CertificateTypeController(ICertificateType certificateTypeService)
    {
        _certificateTypeService = certificateTypeService;
    }

    #region CRUD Operations

    /// <summary>
    /// Create or update a certificate type
    /// </summary>
    /// <param name="request">Certificate type information</param>
    /// <returns>Certificate type view model</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Result<CertificateTypeViewModel>> UpsertCertificateType(
        UpsertCertificateTypeRequest request)
    {
        return await _certificateTypeService.UpsertCertificateType(request, UserId);
    }

    /// <summary>
    /// Get certificate type by ID
    /// </summary>
    /// <param name="certificateTypeId">Certificate type ID</param>
    /// <returns>Certificate type view model</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<Result<CertificateTypeViewModel>> GetCertificateType(
        [FromQuery] long certificateTypeId)
    {
        return await _certificateTypeService.GetType(certificateTypeId, UserId);
    }

    /// <summary>
    /// Get paginated list of certificate types
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of certificate types</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,User")]
    public async Task<Result<PagedResult<CertificateTypeViewModel>>> GetCertificateTypes(
        PagedRequest request)
    {
        return await _certificateTypeService.GetTypes(request, UserId);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle certificate type activation status (activate/deactivate)
    /// </summary>
    /// <param name="certificateTypeId">Certificate type ID</param>
    /// <returns>Updated certificate type view model</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Result<CertificateTypeViewModel>> ToggleActivation(
        [FromQuery] long certificateTypeId)
    {
        return await _certificateTypeService.ToggleCertificateTypeActivation(certificateTypeId, UserId);
    }

    #endregion
}
