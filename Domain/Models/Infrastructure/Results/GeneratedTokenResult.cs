namespace Domain.Models.Infrastructure.Results;

public record GeneratedTokenResult(
    string AccessToken, 
    DateTime AccessTokenExpiry,
    string RefreshToken,
    DateTime RefreshTokenExpiry);