using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class UserProfileResult
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("first_name")]
    public string FirstName { get; set; } = default!;

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("updated_date")]
    public DateTime? UpdatedDate { get; set; }

    [JsonProperty("status")]
    public byte Status { get; set; }
}
