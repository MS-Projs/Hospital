namespace Domain.Models.Integration.Sms;

public record SendSmsParams(string Otp, string Phone, string Message);