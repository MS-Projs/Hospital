using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using DataAccess.Schemas.Auth;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Public;

[Table("patients", Schema = "public")]
[Index(nameof(UserId), IsUnique = true)]
public class Patient : Entity
{
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    [Column("full_name")]
    public string FullName { get; set; } = default!;

    [Range(0, 120)]
    [Column("age")]
    public int Age { get; set; }

    [Required]
    [MaxLength(16)]
    [Column("gender")]
    public string Gender { get; set; } = default!;

    [MaxLength(200)]
    [Column("address")]
    public string? Address { get; set; }

    [Column("additional_notes")]
    public string? AdditionalNotes { get; set; }
}