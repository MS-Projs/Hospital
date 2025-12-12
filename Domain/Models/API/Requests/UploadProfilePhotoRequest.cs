using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UploadProfilePhotoRequest
{
    [JsonIgnore]
    [Required]
    [JsonProperty("user_id")]
    public long UserId { get; set; }

    [Required]
    [JsonProperty("photo")]
    public IFormFile Photo { get; set; } = default!;
}