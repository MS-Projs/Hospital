using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, tokenParams.Id.ToString()),
                new(JwtRegisteredClaimNames.GivenName, tokenParams.FirstName),
                new(JwtRegisteredClaimNames.FamilyName, tokenParams.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.MobilePhone, tokenParams.Phone),
                new(ClaimTypes.Role, tokenParams.Role)
            };

            var token = new JwtSecurityToken(jwtOptions.Value.Issuer, jwtOptions.Value.Audience, claims,
                expires: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationInMinutes),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var expiry = token.ValidTo;

            return Task.FromResult<Result<GeneratedTokenResult>>(new GeneratedTokenResult(tokenString, expiry));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error generating token for user {UserId}", tokenParams.Id);
            return Task.FromResult<Result<GeneratedTokenResult>>(new ErrorModel(ErrorEnum.InternalServerError));
        }
    }
}