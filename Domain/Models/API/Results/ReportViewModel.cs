using DataAccess.Schemas.Public;

namespace Domain.Models.API.Results;

public class ReportViewModel
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public string PatientPhone { get; set; } = default!;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public string DoctorSpecialization { get; set; } = default!;
    public string ReportText { get; set; } = default!;
    public long? AppointmentId { get; set; }
    public string? PdfPath { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime ReportDate { get; set; }
    public DateTime CreatedDate { get; set; }

    public ReportViewModel() { }

    public ReportViewModel(Report report, string baseUrl)
    {
        Id = report.Id;
        PatientId = report.PatientId;
        PatientName = $"{report.Patient?.User?.FirstName} {report.Patient?.User?.LastName}".Trim();
        PatientPhone = report.Patient?.User?.Phone ?? "";
        DoctorId = report.DoctorId;
        DoctorName = report.Doctor?.FullName ?? "";
        DoctorSpecialization = report.Doctor?.Specialization ?? "";
        ReportText = report.ReportText;
        AppointmentId = report.AppointmentId;
        PdfPath = report.PdfPath;
        DownloadUrl = !string.IsNullOrEmpty(report.PdfPath) ? $"{baseUrl}/api/report/{report.Id}/download" : null;
        Notes = report.Notes;
        ReportDate = report.ReportDate;
        CreatedDate = report.CreatedDate;
    }
}
