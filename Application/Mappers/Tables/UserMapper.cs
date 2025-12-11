using DataAccess.Enums;
using DataAccess.Schemas.Auth;
using Domain.Models.API.Requests;
using Mapster;

namespace Application.Mappers.Tables;

public class UserMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSessionRequest, User>().ConstructUsing(src => Map(src));
    }

    private static User Map(CreateSessionRequest source)
    {
        return new User
        {
           
            Phone = source.Phone,
            Role = Role.User
        };
    }
}