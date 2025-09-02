namespace Domain.Models.API.Results;

public record SignInResult(
    string Token,
    DateTime ExpireDate);