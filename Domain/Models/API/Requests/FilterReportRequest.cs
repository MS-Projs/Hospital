using Domain.Models.Common;

namespace Domain.Models.API.Requests;

public record FilterReportRequest : PagedRequest
{
    public long? PatientId { get; set; }
    public long? DoctorId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchText { get; set; }
}
