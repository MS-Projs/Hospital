using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class ChangePasswordRequest
{
    [Required]
    [JsonProperty("current_password")]
    public string CurrentPassword { get; set; } = default!;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    [JsonProperty("new_password")]
    public string NewPassword { get; set; } = default!;

    [Required]
    [Compare(nameof(NewPassword))]
    [JsonProperty("confirm_password")]
    public string ConfirmPassword { get; set; } = default!;
}
