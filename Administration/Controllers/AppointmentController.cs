using Administration.Helpers;
using Application.Interfaces;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Administration.Controllers;


[ApiController]
[Route("api/v1/[controller]/[action]")]
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
    
    #region Appointment Report Management

    /// <summary>
    /// Create a new medical report for a patient appointment
    /// </summary>
    /// <param name="request">Report creation request containing patient, doctor, and report details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created report details</returns>
    [HttpPost]
    public async Task<Result<ReportViewModel>> CreateReport(
        [FromForm] CreateReportRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _reportService.CreateReport(request, cancellationToken);
    }

    /// <summary>
    /// Get a specific medical report by ID
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report details</returns>
    [HttpGet]
    public async Task<Result<ReportViewModel>> GetReport(
        [FromQuery] long reportId,
        CancellationToken cancellationToken = default)
    {
        return await _reportService.GetReportById(reportId, cancellationToken);
    }

    /// <summary>
    /// Get filtered list of medical reports
    /// </summary>
    /// <param name="request">Filter parameters including patient ID, doctor ID, date range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of medical reports</returns>
    [HttpGet]
    public async Task<Result<PagedResult<ReportViewModel>>> GetReports(
        [FromQuery] FilterReportRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _reportService.GetReports(request, cancellationToken);
    }

    /// <summary>
    /// Upload or update a PDF file for an existing report
    /// </summary>
    /// <param name="request">Report file upload request containing report ID and file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated report details</returns>
    [HttpPost]
    public async Task<Result<ReportViewModel>> UploadReportFile(
        [FromForm] UploadReportFileRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _reportService.UploadReportFile(request, cancellationToken);
    }

    /// <summary>
    /// Download a medical report PDF file
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report PDF file stream</returns>
    [HttpGet]
    public async Task<IActionResult> DownloadReport(
        [FromQuery] long reportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.DownloadReportPdf(reportId, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        var (stream, fileName, contentType) = result.Payload!;
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Delete a medical report (soft delete)
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete]
    public async Task<Result<bool>> DeleteReport(
        [FromQuery] long reportId,
        CancellationToken cancellationToken = default)
    {
        return await _reportService.DeleteReport(reportId, cancellationToken);
    }

    #endregion 
}