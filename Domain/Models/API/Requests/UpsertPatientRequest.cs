using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Schemas.Auth;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpsertPatientRequest
{
    [JsonProperty("id")]
    public long? Id { get; set; }
    
    [JsonProperty("user_id")]
    public long UserId { get; set; }

    [Required]
    [MaxLength(200)]
    [JsonProperty("full_name")]
    public string FullName { get; set; } = default!;

    [Range(0, 120)]
    [JsonProperty("age")]
    public int Age { get; set; }

    [Required]
    [MaxLength(16)]
    [JsonProperty("gender")]
    public string Gender { get; set; } = default!;

    [MaxLength(200)]
    [JsonProperty("address")]
    public string? Address { get; set; }

    [JsonProperty("additional_notes")]
    public string? AdditionalNotes { get; set; }
}