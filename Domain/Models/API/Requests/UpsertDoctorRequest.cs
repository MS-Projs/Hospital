using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Schemas.Auth;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpsertDoctorRequest
{
    [JsonProperty("id")] 
    public long Id { get; set; } = 0;
    
    [Column("user_id")]
    public long UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    [Column("full_name")]
    public string FullName { get; set; } 
    
    [Required]
    [MaxLength(100)]
    [Column("specialization")]
    public string Specialization { get; set; }

    [Range(0, 80)]
    [Column("experience_years")]
    public int ExperienceYears { get; set; }

    [MaxLength(200)]
    [Column("workplace")]
    public string? Workplace { get; set; }    

    [Column("biography")]
    public string? Biography { get; set; }
}