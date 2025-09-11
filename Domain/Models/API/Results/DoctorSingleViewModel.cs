using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Schemas.Auth;
using DataAccess.Schemas.Public;
using Domain.Models.Infrastructure.Results;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class DoctorSingleViewModel
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    [JsonProperty("user_id")]
    public long UserId { get; set; }

    [Required]
    [MaxLength(200)]
    [JsonProperty("full_name")]
    public string FullName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    [JsonProperty("specialization")]
    public string Specialization { get; set; } = default!;

    [Range(0, 80)]
    [JsonProperty("experience_years")]
    public int ExperienceYears { get; set; }

    [MaxLength(200)]
    [JsonProperty("workplace")]
    public string? Workplace { get; set; }    

    [JsonProperty("biography")]
    public string? Biography { get; set; }
    
    [JsonProperty("certificates")]
    public List<FileViewModel>? Certificates { get; set; } = new();

    public DoctorSingleViewModel(Doctor doctor)
    {
        Id = doctor.Id;
        UserId = doctor.UserId;
        FullName = doctor.FullName;
        Specialization = doctor.Specialization;
        ExperienceYears = doctor.ExperienceYears;
        Workplace = doctor.Workplace;
        Biography = doctor.Biography;
    }
}