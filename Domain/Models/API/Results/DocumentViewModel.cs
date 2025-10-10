using DataAccess.Schemas.Public;

namespace Domain.Models.API.Results;

public class DocumentViewModel
{
    public long Id { get; set; }
    public string FileName { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public string? CategoryName { get; set; }
    public DateTime UploadedAt { get; set; }
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = default!;

    public DocumentViewModel() { }

    public DocumentViewModel(PatientDocument document, string baseUrl)
    {
        Id = document.Id;
        FileName = Path.GetFileName(document.FilePath);
        FileType = document.FileType;
        CategoryName = document.DocumentCategory?.ValueEn;
        UploadedAt = document.UploadedAt;
        DownloadUrl = $"{baseUrl}/api/patient/document/{document.Id}/download";
    }
}