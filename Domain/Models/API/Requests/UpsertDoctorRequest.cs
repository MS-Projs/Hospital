using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpsertDoctorRequest
{
    [JsonProperty("id")] 
    public long Id { get; set; } = 0;
    
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = default!; 
    
    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = default!;

    [Range(0, 80)]
    public int ExperienceYears { get; set; }

    [MaxLength(200)]
    public string? Workplace { get; set; }    

    public string? Biography { get; set; }
}