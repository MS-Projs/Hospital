namespace Domain.Models.API.Results;

public record SignUpResult(
    string Token, DateTime ExpireDate);