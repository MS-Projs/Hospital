using System.ComponentModel.DataAnnotations;

namespace Domain.Models.API.Requests;

public record SignUpRequest(
    [Required]
    [MaxLength(100)]
    string FirstName,
    
    [Required]
    [MaxLength(100)]
    string LastName,
    
    [Required]
    [Phone]
    string Phone,
    
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    string Password);