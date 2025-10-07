namespace Domain.Models.API.Requests;

public record SignUpRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Password);