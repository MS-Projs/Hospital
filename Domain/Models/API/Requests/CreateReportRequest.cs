using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.API.Requests;

public class CreateReportRequest
{
    [Required]
    public long PatientId { get; set; }

    [Required]
    public long DoctorId { get; set; }

    [Required]
    public string ReportText { get; set; } = default!;

    public long? AppointmentId { get; set; }

    public IFormFile? ReportFile { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
