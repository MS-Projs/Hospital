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

namespace Application.Services;

public class AppointmentService(EntityContext context) : IAppointment
{
    public async Task<Result<AppointmentViewModel>> CreateAppointment(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify patient exists
            var patient = await context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId && p.Status != EntityStatus.Deleted, cancellationToken);
                
            if (patient == null)
                return new ErrorModel(ErrorEnum.PatientNotFound);

            // Verify doctor exists
            var doctor = await context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.Status != EntityStatus.Deleted, cancellationToken);
                
            if (doctor == null)
                return new ErrorModel(ErrorEnum.DoctorNotFound);

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                Message = request.Message,
                PreferredDate = request.PreferredDate,
                Status = AppointmentStatus.Pending,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.Appointments.AddAsync(appointment, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Load with includes for view model
            var savedAppointment = await context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstAsync(a => a.Id == appointment.Id, cancellationToken);

            return new AppointmentViewModel(savedAppointment);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<AppointmentViewModel>> UpdateAppointmentStatus(UpdateAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.Status != EntityStatus.Deleted, cancellationToken);
                
            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            appointment.Status = request.Status;
            appointment.Notes = request.Notes;
            appointment.ScheduledDate = request.ScheduledDate;
            appointment.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return new AppointmentViewModel(appointment);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<AppointmentViewModel>> GetAppointmentById(long appointmentId, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Status != EntityStatus.Deleted, cancellationToken);
                
            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            return new AppointmentViewModel(appointment);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<PagedResult<AppointmentViewModel>>> GetAppointments(FilterAppointmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.Status != EntityStatus.Deleted);

            // Apply filters
            if (request.PatientId.HasValue)
                query = query.Where(a => a.PatientId == request.PatientId.Value);

            if (request.DoctorId.HasValue)
                query = query.Where(a => a.DoctorId == request.DoctorId.Value);

            if (request.Status.HasValue)
                query = query.Where(a => a.Status == request.Status.Value);

            if (request.FromDate.HasValue)
                query = query.Where(a => a.CreatedDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(a => a.CreatedDate <= request.ToDate.Value);

            var appointments = await query
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync(cancellationToken);

            var appointmentViewModels = appointments.Select(a => new AppointmentViewModel(a)).ToList();

            return request.All ? 
                appointmentViewModels.ToListResponse() : 
                appointmentViewModels.ToListResponse(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> CancelAppointment(long appointmentId, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Status != EntityStatus.Deleted, cancellationToken);
                
            if (appointment == null)
                return new ErrorModel(ErrorEnum.AppointmentNotFound);

            appointment.Status = AppointmentStatus.Rejected;
            appointment.UpdatedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }
}