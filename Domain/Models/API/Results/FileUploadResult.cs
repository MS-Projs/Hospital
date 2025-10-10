namespace Domain.Models.API.Results;

public class FileUploadResult
{
    public long Id { get; set; }
    public string FileName { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }

    public FileUploadResult()
    {
        Success = true;
        UploadedAt = DateTime.UtcNow;
    }

    public FileUploadResult(string errorMessage)
    {
        Success = false;
        Message = errorMessage;
    }
}