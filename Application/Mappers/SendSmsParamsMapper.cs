using DataAccess.Schemas.Auth;
using Domain.Models.Integration.Sms;
using Mapster;

namespace Application.Mappers;

public class SendSmsParamsMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<(User, Session), SendSmsParams>()
            .ConstructUsing(src => Map(src));
    }

    private static SendSmsParams Map((User, Session) src)
    {
        return new SendSmsParams(
            Otp: src.Item2.OtpCode,
            Phone: src.Item1.Phone,
            Message: $"Код подтверждения для авторизации на сайте MyMd.uz: {src.Item2.OtpCode} RQaLW8tPrW1");
    }

}