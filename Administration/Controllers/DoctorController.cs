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
[Authorize]
public class DoctorController : MyController<DoctorController>
{
    private readonly IDoctor _doctorService;

    public DoctorController(IDoctor doctorService)
    {
        _doctorService = doctorService;
    }

    #region CRUD Operations

    /// <summary>
    /// Create or update a doctor
    /// </summary>
    /// <param name="request">Doctor information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Doctor view model</returns>
    [HttpPost]
    public async Task<Result<DoctorViewModel>> UpsertDoctor(
        UpsertDoctorRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.UpsertDoctor(request, cancellationToken);
    }

    /// <summary>
    /// Get doctor by ID
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Doctor single view model</returns>
    [HttpGet]
    public async Task<Result<DoctorSingleViewModel>> GetDoctorById(
        [FromQuery] long doctorId, 
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctorById(doctorId, cancellationToken);
    }

    /// <summary>
    /// Get paginated list of doctors with optional filtering
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of doctors</returns>
    [HttpPost]
    public async Task<Result<PagedResult<DoctorViewModel>>> GetDoctors(
        FilterDoctorRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctors(request, cancellationToken);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle doctor activation status (activate/deactivate)
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated doctor view model</returns>
    [HttpPost]
    public async Task<Result<DoctorViewModel>> ToggleActivation(
        [FromQuery] long doctorId, 
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.DoctorToggleActivation(doctorId, cancellationToken);
    }

    #endregion
}
