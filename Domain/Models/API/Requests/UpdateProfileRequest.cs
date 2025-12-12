using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpdateProfileRequest
{
    [JsonProperty("user_id")]
    [JsonIgnore]
    public long UserId { get; set; }

    [MaxLength(100)]
    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [MaxLength(32)]
    [Phone]
    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonProperty("gender")]
    public Gender? Gender { get; set; }

    [MaxLength(500)]
    [JsonProperty("address")]
    public string? Address { get; set; }
}