namespace Domain.Models.API.Results;

public record RefreshTokenResult(
    string AccessToken,
    DateTime AccessTokenExpiry,
    string RefreshToken,
    DateTime RefreshTokenExpiry);




