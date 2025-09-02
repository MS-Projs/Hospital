using DataAccess.Schemas.Auth;
using Domain.Models.API.Requests;
using Mapster;

namespace Application.Mappers.Tables;
/*
public class UserMapper : IRegister
{
public void Register(TypeAdapterConfig config)
{
config.NewConfig<SignUpRequest, User>().ConstructUsing(src => Map(src));
}

private static User Map(SignUpRequest source)
{
return new User
{
FirstName = source.FirstName,
LastName = source.LastName,
Phone = source.Phone,
Email = source.Email
};
}
}*/