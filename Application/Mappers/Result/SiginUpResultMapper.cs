
using Domain.Models.API.Results;
using Domain.Models.Infrastructure.Results;
using Mapster;

namespace Application.Mappers.Result;

public class SiginUpResultMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<GeneratedTokenResult, SignUpResult>()
            .ConstructUsing(src => Map(src));
    }

    private SignUpResult Map(GeneratedTokenResult src)
    {
        return new SignUpResult(src.Token, src.ExpireDate);
    }
}