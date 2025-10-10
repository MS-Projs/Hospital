using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.API.Requests;

public class UploadPatientDocumentRequest
{
    [Required]
    public long PatientId { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    public string FileType { get; set; } = default!; // MedicalRecord, TestResult, Prescription, etc.

    public long? CategoryId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
