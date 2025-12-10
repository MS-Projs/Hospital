using Domain.Models.Common;
using Domain.Models.Integration.Sms;

namespace Infrastructure.Interfaces;

public interface ISmsService
{
    Task<Result<SendSmsResult>> SendSms(SendSmsParams sendSmsParams);
}