using DataAccess.Schemas.Auth;
using Domain.Models.Infrastructure.Params;
using Mapster;

namespace Application.Mappers;

public class GenerateTokenParamsMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<User, GenerateTokenParams>()
            .ConstructUsing(src => Map(src));
    }

    private static GenerateTokenParams Map(User source)
    {
        return new GenerateTokenParams(
            source.Id,
            source.FirstName,
            source.LastName,
            source.Phone,
            "user",
            source.Email);
    }
}