using System.ComponentModel.DataAnnotations;

namespace Domain.Models.API.Requests;

public record SignInRequest(
    [Required]
    [Phone]
    string Phone,
    
    [Required]
    string Password);