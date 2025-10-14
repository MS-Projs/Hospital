using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Enums;
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

    [MaxLength(100)]
    [Column("last_name")]
    public string? LastName { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("password")]
    public string Password { get; set; } = default!;

    [Required]
    [MaxLength(32)]
    [Column("phone")]
    public string Phone { get; set; } = default!;

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("gender")]
    public Gender? Gender { get; set; }

    [MaxLength(500)]
    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(500)]
    [Column("profile_photo_path")]
    public string? ProfilePhotoPath { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}