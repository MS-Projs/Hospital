using System.ComponentModel.DataAnnotations;

namespace Domain.Models.API.Requests;

public class CreateAppointmentRequest
{
    [Required]
    public long PatientId { get; set; }

    [Required]
    public long DoctorId { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    public DateTime? ScheduledDate { get; set; }
    
    public int Duration { get; set; }
}
