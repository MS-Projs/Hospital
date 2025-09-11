using System.ComponentModel.DataAnnotations;
using DataAccess.Schemas.Public;
using Domain.Models.Infrastructure.Results;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class PatientSingleViewModel
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
    
    [MaxLength(200)]
    [JsonProperty("address")]
    public string? Address { get; set; }

    [JsonProperty("additional_notes")]
    public string? AdditionalNotes { get; set; }
    
    [JsonProperty("documents")]
    public List<FileViewModel>? Documents { get; set; } = new List<FileViewModel>();

    public PatientSingleViewModel( Patient patient)
    {
        Id = patient.Id;
        UserId = patient.UserId;
        FullName = patient.FullName;
        Age = patient.Age;
        Gender = patient.Gender;
        Address = patient.Address;
        AdditionalNotes = patient.AdditionalNotes;
        
    }
}