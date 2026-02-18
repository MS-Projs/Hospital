namespace Domain.Models.API.Requests;

public record RejectDoctorRequest(long DoctorId , string? Reason);