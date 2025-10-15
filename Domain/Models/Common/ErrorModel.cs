using Domain.Enums;

namespace Domain.Models.Common;

public class ErrorModel
{
    public string Code { get; set; }
    public string? Message { get; set; }
    public object? Details { get; set; }  // ← Add this property

    public ErrorModel(ErrorEnum errorEnum)
    {
        Code = errorEnum.ToString();
        Message = string.Empty;
    }

    public ErrorModel(ErrorEnum errorEnum, string message)
    {
        Code = errorEnum.ToString();
        Message = message;
    }

    public ErrorModel(ErrorEnum errorEnum, string message, object? details)  // ← Add this constructor
    {
        Code = errorEnum.ToString();
        Message = message;
        Details = details;
    }
}