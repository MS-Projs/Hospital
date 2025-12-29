using System.ComponentModel.DataAnnotations;
using Domain.Models.Common;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public record FilterDoctorRequest : PagedRequest
{
    [Required]
    [MaxLength(200)]
    [JsonProperty("full_name")]
    public string? FullName { get; set; } 
    
    [Required]
    [MaxLength(100)]
    [JsonProperty("specialization")]
    public string? Specialization { get; set; }

    [Range(0, 80)]
    [JsonProperty("experience_years")]
    public int? ExperienceYears { get; set; }

}