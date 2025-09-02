namespace Domain.Models.API.Requests;

public record SignInRequest(
    string Phone,
    string Password);