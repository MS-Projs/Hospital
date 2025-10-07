using Domain.Models.Common;
using Domain.Models.Infrastructure.Params;
using Domain.Models.Infrastructure.Results;

namespace Infrastructure.Interfaces;

public interface ITokenService
{
    Task<Result<GeneratedTokenResult>> GenerateToken(GenerateTokenParams tokenParams);
}