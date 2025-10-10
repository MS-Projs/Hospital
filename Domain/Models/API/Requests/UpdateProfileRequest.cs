using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    [JsonProperty("first_name")]
    public string FirstName { get; set; } = default!;

    [MaxLength(100)]
    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [MaxLength(32)]
    [JsonProperty("phone")]
    public string? Phone { get; set; }
}
