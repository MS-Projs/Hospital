using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.API.Requests;

public class UploadDoctorCertificateRequest
{
    [Required]
    public long DoctorId { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    public string FileType { get; set; } = default!; // Certificate, License, Diploma, etc.

    [Required]
    public long CategoryId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}