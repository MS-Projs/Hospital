namespace Domain.Models.Common;

public class Result<T>
{
    public Result(T payload)
    {
        Payload = payload;
    }

    public Result(ErrorModel errorModel)
    {
        Error = errorModel;
        Success = false;
    }

    public T? Payload { get; set; }
    public bool Success { get; set; } = true;
    public ErrorModel? Error { get; set; }

    public static implicit operator Result<T>(T payload)
    {
        return new Result<T>(payload);
    }

    public static implicit operator Result<T>(ErrorModel errorModel)
    {
        return new Result<T>(errorModel);
    }
}