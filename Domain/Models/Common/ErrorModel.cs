using Domain.Enums;

namespace Domain.Models.Common;

public class ErrorModel(
    ErrorEnum errorEnum)
{
    public string Code { get; set; } = errorEnum.ToString();
    public string? Message { get; set; } = string.Empty;
}