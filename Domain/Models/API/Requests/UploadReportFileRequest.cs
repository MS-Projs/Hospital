using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.API.Requests;

public class UploadReportFileRequest
{
    [Required]
    public long ReportId { get; set; }

    [Required]
    public IFormFile File { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }
}