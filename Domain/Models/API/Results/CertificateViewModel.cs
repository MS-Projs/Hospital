using DataAccess.Schemas.Public;

namespace Domain.Models.API.Results;

public class CertificateViewModel
{
    public long Id { get; set; }
    public string FileName { get; set; } = default!;
    public string FileType { get; set; } = default!;
    public string? CategoryName { get; set; }
    public DateTime UploadedAt { get; set; }
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = default!;

    public CertificateViewModel() { }

    public CertificateViewModel(DoctorCertificate certificate, string baseUrl)
    {
        Id = certificate.Id;
        FileName = Path.GetFileName(certificate.FilePath);
        FileType = certificate.FileType;
        CategoryName = certificate.CertificateType?.ValueEn;
        UploadedAt = certificate.UploadedAt;
        DownloadUrl = $"{baseUrl}/api/doctor/certificate/{certificate.Id}/download";
    }
}