namespace Domain.Models.Infrastructure.Params;

public record GenerateTokenParams(
    long Id,
    string FirstName,
    string? LastName,
    string? Phone,
    string Role,
    string? Email);