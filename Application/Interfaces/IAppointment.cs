using Domain.Models.API.Requests;
using Domain.Models.API.Results;
using Domain.Models.Common;

namespace Application.Interfaces;

public interface IAppointment
{
    Task<Result<AppointmentViewModel>> CreateAppointment(CreateAppointmentRequest request, CancellationToken cancellationToken);
    Task<Result<AppointmentViewModel>> UpdateAppointmentStatus(UpdateAppointmentStatusRequest request, CancellationToken cancellationToken);
    Task<Result<AppointmentViewModel>> GetAppointmentById(long appointmentId, CancellationToken cancellationToken);
    Task<Result<PagedResult<AppointmentViewModel>>> GetAppointments(FilterAppointmentRequest request, CancellationToken cancellationToken);
    Task<Result<bool>> CancelAppointment(long appointmentId, CancellationToken cancellationToken);
    Task<Result<bool>> AppointmentToggleActivation(long appointmentId, CancellationToken cancellationToken);

}
