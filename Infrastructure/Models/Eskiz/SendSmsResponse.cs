namespace Infrastructure.Models.Eskiz;

public record SendSmsResponse
{
    public string Id { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}