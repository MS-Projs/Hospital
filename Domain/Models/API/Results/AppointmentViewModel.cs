using DataAccess.Enums;
using DataAccess.Schemas.Public;

namespace Domain.Models.API.Results;

public class AppointmentViewModel
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public string PatientPhone { get; set; } = default!;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public string DoctorSpecialization { get; set; } = default!;
    public string? Message { get; set; }
    public DateTime? PreferredDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public AppointmentStatus AppointmentStatus { get; set; }
    public string StatusName { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    public EntityStatus IsDeleted { get; set; }

    public AppointmentViewModel() { }

    public AppointmentViewModel(Appointment appointment)
    {
        Id = appointment.Id;
        PatientId = appointment.PatientId;
        PatientName = $"{appointment.Patient?.User?.FirstName} {appointment.Patient?.User?.LastName}".Trim();
        PatientPhone = appointment.Patient?.User?.Phone ?? "";
        DoctorId = appointment.DoctorId;
        DoctorName = appointment.Doctor?.FullName ?? "";
        DoctorSpecialization = appointment.Doctor?.Specialization ?? "";
        Message = appointment.Message;
        PreferredDate = appointment.PreferredDate;
        ScheduledDate = appointment.ScheduledDate;
        AppointmentStatus = appointment.AppointmentStatus;
        StatusName = appointment.Status.ToString();
        CreatedDate = appointment.CreatedDate;
        UpdatedDate = appointment.UpdatedDate;
        IsDeleted = appointment.Status;

    }
}