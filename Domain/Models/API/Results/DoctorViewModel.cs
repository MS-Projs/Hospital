using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class DoctorViewModel
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
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
    
    [Column("status")]
    public EntityStatus Status { get; set; }

    public DoctorViewModel( Doctor doctor)
    {
        Id = doctor.Id;
        FullName = doctor.FullName;
        Specialization = doctor.Specialization;
        ExperienceYears = doctor.ExperienceYears;
        Status = doctor.Status;
    }
    
}