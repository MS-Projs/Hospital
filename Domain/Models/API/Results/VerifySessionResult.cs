namespace Domain.Models.API.Results;

public record VerifySessionResult(
    string AccessToken,
    DateTime AccessTokenExpiry,
    string RefreshToken,
    DateTime RefreshTokenExpiry);
