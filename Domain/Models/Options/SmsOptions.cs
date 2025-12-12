namespace Domain.Models.Options;

public class SmsOptions
{
    public static string SectionName = "SmsOptions";
    public string Username { get; set; }
    public string Password { get; set; }
    public string BaseUrl { get; set; }
}
