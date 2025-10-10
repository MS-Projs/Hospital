using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace Domain.Models.API.Requests;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public long AppointmentId { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime? ScheduledDate { get; set; }
}
