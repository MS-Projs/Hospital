using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;

public class AppointmentController : MyController<AppointmentController>
{
    private readonly IAppointment _appointmentService;
    private readonly IReport _reportService;

    public AppointmentController(IAppointment appointmentService, IReport reportService)
    {
        _appointmentService = appointmentService;
        _reportService = reportService;
    }

    #region CRUD Operations

    /// <summary>
    /// Create or update a Appointment
    /// </summary>
    /// <param name="request">Appointment information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Appointment view model</returns>
    [HttpPost]
    public async Task<Result<AppointmentViewModel>> UpsertAppointment(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.CreateAppointment(request, cancellationToken);
    }

    /// <summary>
    /// Get Appointment by ID
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Appointment single view model</returns>
    [HttpGet]
    public async Task<Result<AppointmentViewModel>> GetAppointmentById(
        [FromQuery] long appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.GetAppointmentById(appointmentId, cancellationToken);
    }

    /// <summary>
    /// Get paginated list of Appointments with optional filtering
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of Appointments</returns>
    [HttpPost]
    public async Task<Result<PagedResult<AppointmentViewModel>>> GetAppointments(
        FilterAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.GetAppointments(request, cancellationToken);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle Appointment activation status (activate/deactivate)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated Appointment view model</returns>
    [HttpPost]
    public async Task<Result<bool>> ToggleActivation(
        [FromQuery] long appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.AppointmentToggleActivation(appointmentId, cancellationToken);
    }

    /// <summary>
    /// Toggle Appointment status (pending / approved / completed)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated Appointment view model</returns>
    [HttpPost]
    public async Task<Result<AppointmentViewModel>> UpdateAppointmentStatus(
        UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.UpdateAppointmentStatus(request, cancellationToken);
    }

    /// <summary>
    /// Toggle Appointment status (pending / approved / completed)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated Appointment view model</returns>
    [HttpPost]
    public async Task<Result<bool>> CancelAppointment(
        [FromQuery] long appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await _appointmentService.CancelAppointment(appointmentId, cancellationToken);
    }

    #endregion
    
    /*#region Doctor Certificate Management

    /// <summary>
    /// Upload a certificate for a doctor
    /// </summary>
    /// <param name="request">Certificate upload request containing doctor ID, file, and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Uploaded certificate details</returns>
    [HttpPost("upload-certificate")]
    public async Task<Result<CertificateViewModel>> UploadCertificate(
        [FromForm] UploadDoctorCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.UploadDoctorCertificate(request, cancellationToken);
    }

    /// <summary>
    /// Get all certificates for a specific doctor
    /// </summary>
    /// <param name="doctorId">Doctor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of doctor certificates</returns>
    [HttpGet("certificates")]
    public async Task<Result<List<CertificateViewModel>>> GetCertificates(
        [FromQuery] long doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.GetDoctorCertificates(doctorId, cancellationToken);
    }

    /// <summary>
    /// Download a specific doctor certificate
    /// </summary>
    /// <param name="certificateId">Certificate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Certificate file stream</returns>
    [HttpGet("download-certificate")]
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
    /// Delete a doctor certificate (soft delete)
    /// </summary>
    /// <param name="certificateId">Certificate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("certificate")]
    public async Task<Result<bool>> DeleteCertificate(
        [FromQuery] long certificateId,
        CancellationToken cancellationToken = default)
    {
        return await _doctorService.DeleteDoctorCertificate(certificateId, cancellationToken);
    }

    #endregion*/
}