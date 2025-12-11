using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Enums;
using Domain.Models.Common;
using Domain.Models.Infrastructure.Params;
using Domain.Models.Infrastructure.Results;
using Domain.Models.Options;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

internal class TokenService(
    IOptions<JwtOptions> jwtOptions,
    ILogger<TokenService> logger) : ITokenService
{
    public Task<Result<GeneratedTokenResult>> GenerateToken(GenerateTokenParams tokenParams)
    {
        try
        {
            // Generate Access Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, tokenParams.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.MobilePhone, tokenParams.Phone),
                new(ClaimTypes.Role, tokenParams.Role)
            };

            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationInMinutes);
            var token = new JwtSecurityToken(
                jwtOptions.Value.Issuer, 
                jwtOptions.Value.Audience, 
                claims,
                expires: accessTokenExpiry,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Generate Refresh Token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationInDays);

            return Task.FromResult<Result<GeneratedTokenResult>>(
                new GeneratedTokenResult(accessToken, accessTokenExpiry, refreshToken, refreshTokenExpiry));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error generating token for user {UserId}", tokenParams.Id);
            return Task.FromResult<Result<GeneratedTokenResult>>(new ErrorModel(ErrorEnum.InternalServerError));
        }
    }
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}