using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Models.Common;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public record FilterPatientRequest : PagedRequest
{
    [JsonProperty("full_name")]
    public string? FullName { get; set; } 

    [Range(0, 120)]
    [JsonProperty("age")]
    public int? Age { get; set; }
    
    [MaxLength(16)]
    [JsonProperty("gender")]
    public string? Gender { get; set; } 
    
}