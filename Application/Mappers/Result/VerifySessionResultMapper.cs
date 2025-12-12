
using Domain.Models.API.Results;
using Domain.Models.Infrastructure.Results;
using Mapster;

namespace Application.Mappers.Result;

public class VerifySessionResultMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<GeneratedTokenResult, VerifySessionResult>()
            .ConstructUsing(src => Map(src));
    }

    private VerifySessionResult Map(GeneratedTokenResult src)
    {
        return new VerifySessionResult(
            src.AccessToken,
            src.AccessTokenExpiry,
            src.RefreshToken,
            src.RefreshTokenExpiry);
    }
}