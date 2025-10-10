using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using DataAccess.Schemas.Public;

namespace DataAccess.Schemas.Public;

[Table("doctor_certificates", Schema = "public")]
public class DoctorCertificate : Entity
{
    [Column("doctor_id")]
    public long DoctorId { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public Doctor Doctor { get; set; } = default!;

    [Required]
    [MaxLength(400)]
    [Column("file_path")]
    public string FilePath { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    [Column("file_type")]
    public string FileType { get; set; } = default!;

    [Column("category")]
    public long Category { get; set; }
    
    [ForeignKey(nameof(Category))]
    public CertificateType CertificateType { get; set; } = default!;

    [Required]
    [MaxLength(512)]
    [Column("encrypted_key")]
    public string EncryptedKey { get; set; } = default!;

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}