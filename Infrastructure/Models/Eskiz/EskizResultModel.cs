namespace Infrastructure.Models.Eskiz;

public class EskizResultModel<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
}