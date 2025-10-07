namespace Domain.Models.Infrastructure.Results;

public record GeneratedTokenResult(
    string Token,
    DateTime ExpireDate);