using DataAccess.Schemas.Auth;
using Domain.Models.API.Results;
using Domain.Models.Infrastructure.Results;
using Mapster;

namespace Application.Mappers.Result;

public class CreateSessionResultMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Session, CreateSessionResult>()
            .ConstructUsing(src => Map(src));
    }

    private CreateSessionResult Map(Session src)
    {
        return new CreateSessionResult(
            SessionId: src.Id,
            ExpireDate: src.OtpExpireDate);
    }
}