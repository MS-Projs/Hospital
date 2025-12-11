namespace Domain.Models.Options;

public class JwtOptions
{
    public static string SectionName = "Jwt";
    public string Key { get; init; } 
    public string Issuer { get; init; } 
    public string Audience { get; init; }
    public int ExpirationInMinutes { get; init; }
    public int RefreshTokenExpirationInDays { get; set; }
}