using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Enums;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Public;

[Table("appointments", Schema = "public")]
[Index(nameof(PatientId))]
[Index(nameof(DoctorId))]
[Index(nameof(Status))]
public class Appointment : Entity
{
    [Column("patient_id")]
    public long PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    [Column("doctor_id")]
    public long DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = default!;

    [MaxLength(500)]
    [Column("message")]
    public string? Message { get; set; }

    [Column("preferred_date")]
    public DateTime? PreferredDate { get; set; }

    [Column("status")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


