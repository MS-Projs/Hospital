using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Schemas.Public;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class PatientViewModel
{
    
    [JsonProperty("id")]
    public long Id { get; set; }
    
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

    public PatientViewModel( Patient patient)
    {
        Id = patient.Id;
        UserId = patient.UserId;
        FullName = patient.FullName;
        Age = patient.Age;
        Gender = patient.Gender;
    }

}