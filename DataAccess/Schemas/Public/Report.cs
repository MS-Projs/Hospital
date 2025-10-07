using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Public;

[Table("reports", Schema = "public")]
[Index(nameof(PatientId))]
[Index(nameof(DoctorId))]
public class Report : Entity
{
    [Column("patient_id")]
    public long PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    [Column("doctor_id")]
    public long DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = default!;

    [Required]
    [Column("report_text", TypeName = "text")]
    public string ReportText { get; set; } = default!;

    [Required]
    [MaxLength(400)]
    [Column("pdf_path")]       
    public string PdfPath { get; set; } = default!;

    [Column("report_date")]
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
}


