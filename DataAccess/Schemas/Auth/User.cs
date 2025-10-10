using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using DataAccess.Schemas.Public;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Auth;

[Table("users", Schema = "auth")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Phone), IsUnique = true)]
public class User : Entity
{
    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string? LastName { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    [Required]
    [MaxLength(500)]  // Increased size for hashed passwords
    [Column("password")]
    public string Password { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    [Column("phone")]
    public string Phone { get; set; } = default!;
    
    
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}