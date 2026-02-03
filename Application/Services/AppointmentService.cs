using Application.Interfaces;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Domain.Enums;
using Domain.Extensions;
using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AppointmentService : IAppointment
{
    private readonly EntityContext _context;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(EntityContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<AppointmentViewModel>> CreateAppointment(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate patient and doctor existence in parallel
            var (patient, doctor) = await ValidatePatientAndDoctor(
                request.PatientId,
                request.DoctorId,
                cancellationToken);

            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            // Validate scheduling conflicts
            if (request.ScheduledDate.HasValue && request.Duration > 0)
            {
                var hasConflict = await CheckScheduleConflict(
                    request.DoctorId,
                    request.ScheduledDate.Value,
                    request.Duration,
                    cancellationToken);

                if (hasConflict)
                    return new ErrorModel(ErrorEnum.BadRequest, "Doctor has schedule conflict at this time");
            }

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Message = request.Message?.Trim(),
                ScheduledDate = request.ScheduledDate,
                Duration = request.Duration,
                AppointmentStatus = AppointmentStatus.Pending,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _context.Appointments.AddAsync(appointment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Load with includes
            var savedAppointment = await GetAppointmentWithIncludes(appointment.Id, cancellationToken);
            return new AppointmentViewModel(savedAppointment!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment for patient {PatientId} and doctor {DoctorId}",
                request.PatientId, request.DoctorId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<AppointmentViewModel>> UpdateAppointmentStatus(
        UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await GetAppointmentWithIncludes(request.AppointmentId, cancellationToken);

            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            // Validate status transition
            if (!IsValidStatusTransition(appointment.AppointmentStatus, request.Status))
                return new ErrorModel(ErrorEnum.BadRequest, "Invalid status transition");

            appointment.AppointmentStatus = request.Status;
            appointment.Notes = request.Notes?.Trim();
            appointment.ScheduledDate = request.ScheduledDate ?? appointment.ScheduledDate;
            appointment.Duration = request.Duration > 0 ? request.Duration : appointment.Duration;
            appointment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new AppointmentViewModel(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId} status", request.AppointmentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<AppointmentViewModel>> GetAppointmentById(
        long appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await GetAppointmentWithIncludes(appointmentId, cancellationToken);

            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            return new AppointmentViewModel(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {AppointmentId}", appointmentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<AppointmentViewModel>>> GetAppointments(
        FilterAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = BuildAppointmentQuery(request);

            var appointments = await query
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync(cancellationToken);

            var appointmentViewModels = appointments
                .Select(a => new AppointmentViewModel(a))
                .ToList();

            return request.All
                ? appointmentViewModels.ToListResponse()
                : appointmentViewModels.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointments with filter");
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> CancelAppointment(
        long appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Status != EntityStatus.Deleted,
                    cancellationToken);

            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            // Only allow cancellation of pending or approved appointments
            if (appointment.AppointmentStatus == AppointmentStatus.Completed)
                return new ErrorModel(ErrorEnum.BadRequest, "Cannot cancel completed appointment");

            appointment.AppointmentStatus = AppointmentStatus.Rejected;
            appointment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> AppointmentToggleActivation(
        long appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            appointment.Status = appointment.Status == EntityStatus.Deleted
                ? EntityStatus.Active
                : EntityStatus.Deleted;
            appointment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling appointment {AppointmentId} activation", appointmentId);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    #region Private Helper Methods

    private async Task<(Patient? patient, Doctor? doctor)> ValidatePatientAndDoctor(
        long patientId,
        long doctorId,
        CancellationToken cancellationToken)
    {
        var patientTask = _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId && p.Status != EntityStatus.Deleted,
                cancellationToken);

        var doctorTask = _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == doctorId && d.Status != EntityStatus.Deleted,
                cancellationToken);

        await Task.WhenAll(patientTask, doctorTask);

        return (await patientTask, await doctorTask);
    }

    private async Task<Appointment?> GetAppointmentWithIncludes(
        long appointmentId,
        CancellationToken cancellationToken)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Status != EntityStatus.Deleted,
                cancellationToken);
    }

    private IQueryable<Appointment> BuildAppointmentQuery(FilterAppointmentRequest request)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.Status != EntityStatus.Deleted)
            .AsQueryable();

        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);

        if (request.Status.HasValue)
            query = query.Where(a => a.AppointmentStatus == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(a => a.CreatedDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(a => a.CreatedDate <= request.ToDate.Value);

        return query;
    }

    private async Task<bool> CheckScheduleConflict(
        long doctorId,
        DateTime scheduledDate,
        int duration,
        CancellationToken cancellationToken)
    {
        var appointmentEnd = scheduledDate.AddMinutes(duration);

        return await _context.Appointments
            .AnyAsync(a =>
                    a.DoctorId == doctorId &&
                    a.Status != EntityStatus.Deleted &&
                    a.AppointmentStatus != AppointmentStatus.Rejected &&
                    a.ScheduledDate.HasValue &&
                    (
                        // New appointment starts during existing appointment
                        (scheduledDate >= a.ScheduledDate.Value &&
                         scheduledDate < a.ScheduledDate.Value.AddMinutes(a.Duration)) ||
                        // New appointment ends during existing appointment
                        (appointmentEnd > a.ScheduledDate.Value &&
                         appointmentEnd <= a.ScheduledDate.Value.AddMinutes(a.Duration)) ||
                        // New appointment completely overlaps existing appointment
                        (scheduledDate <= a.ScheduledDate.Value &&
                         appointmentEnd >= a.ScheduledDate.Value.AddMinutes(a.Duration))
                    ),
                cancellationToken);
    }

    private static bool IsValidStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        return (current, next) switch
        {
            (AppointmentStatus.Pending, AppointmentStatus.Approved) => true,
            (AppointmentStatus.Pending, AppointmentStatus.Rejected) => true,
            (AppointmentStatus.Approved, AppointmentStatus.Completed) => true,
            (AppointmentStatus.Approved, AppointmentStatus.Rejected) => true,
            _ when current == next => true,
            _ => false
        };
    }

    #endregion
}