namespace Domain.Models.API.Requests;

public record DoctorWorkplaceRequest
{
    public string ClinicName { get; set; } = default!;
    public string ClinicAddress { get; set; } = default!;
}