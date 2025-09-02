using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Public;

[Table("patient_documents", Schema = "public")]
[Index(nameof(PatientId))]
public class PatientDocument : Entity
{
    [Column("patient_id")]
    public long PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = default!;

    [Required]
    [MaxLength(400)]
    [Column("file_path")]
    public string FilePath { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    [Column("file_type")]
    public string FileType { get; set; } = default!; // MR, Tahlil, Recept

    [MaxLength(64)]
    [Column("category")]
    public string? Category { get; set; }

    [Required]
    [MaxLength(512)]
    [Column("encrypted_key")]
    public string EncryptedKey { get; set; } = default!;

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}


