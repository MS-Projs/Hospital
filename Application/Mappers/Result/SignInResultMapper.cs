using Domain.Models.API.Results;
using Domain.Models.Infrastructure.Results;
using Mapster;

namespace Application.Mappers.Result;

public class SignInResultMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<GeneratedTokenResult, SignInResult>()
            .ConstructUsing(src => Map(src));
    }

    private static SignInResult Map(GeneratedTokenResult src)
    {
        return new SignInResult(src.Token, src.ExpireDate);
    }
}