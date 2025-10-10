using DataAccess.Enums;
using Domain.Models.Common;

namespace Domain.Models.API.Requests;

public record FilterAppointmentRequest : PagedRequest
{
    public long? PatientId { get; set; }
    public long? DoctorId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
