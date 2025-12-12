using DataAccess.Schemas.Auth;
using Mapster;

namespace Application.Mappers.Tables;

public class SessionMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, Session>()
            .ConstructUsing(src => Map(src));
    }

    private static Session Map(User src)
    {
        return new Session
        {
            UserId = src.Id,
            OtpCode = GenerateRandomCode(),
            OtpExpireDate = DateTime.UtcNow.AddMinutes(3),
            CreatedDate = DateTime.UtcNow,
            Status = DataAccess.Enums.EntityStatus.Active
        };
    }
    
    private static string GenerateRandomCode()
    {
        
        var random = new Random();
        return "0123123"; //random.Next(1002, 9999).ToString();
    }
}