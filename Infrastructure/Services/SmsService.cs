using Domain.Enums;
using Domain.Models.Common;
using Domain.Models.Integration.Sms;
using Domain.Models.Options;
using Infrastructure.Interfaces;
using Infrastructure.Models.Eskiz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class SmsService(IEskizClient eskizClient, ILogger<SmsService> logger, IOptions<SmsOptions> smsOptions) : ISmsService
{
    public async Task<Result<SendSmsResult>> SendSms(SendSmsParams sendSmsParams)
    {
        try
        {
            var token = await Login();
            if(token == null)
                return new ErrorModel(ErrorEnum.InternalServerError);

            await eskizClient.Send<EskizResultModel<SendSmsResponse>>(
                url: $"{smsOptions.Value.BaseUrl}/message/sms/send",
                forms: new Dictionary<string, string>
                {
                    { "mobile_phone", sendSmsParams.Phone },
                    { "message", sendSmsParams.Message },
                    { "from", "4546" }
                },
                headers: new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token.Data.Token}" }
                });

            return new SendSmsResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending SMS to {Phone}", sendSmsParams.Phone);
            throw;
        }
    }
    
    private async Task<EskizResultModel<LoginResponse>?> Login()
    {
        return await eskizClient.PostAsync<EskizResultModel<LoginResponse>>($"{smsOptions.Value.BaseUrl}/auth/login",
            forms: new Dictionary<string, string>
            {
                { "email", smsOptions.Value.Username },
                { "password", smsOptions.Value.Password }
            });
    }
}