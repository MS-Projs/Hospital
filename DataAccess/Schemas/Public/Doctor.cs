using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models;
using DataAccess.Schemas.Auth;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Schemas.Public;

[Table("doctors", Schema = "public")]
[Index(nameof(UserId), IsUnique = true)]
public class Doctor : Entity
{
    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    [Column("full_name")]
    public string FullName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    [Column("specialization")]
    public string Specialization { get; set; } = default!;

    [Range(0, 80)]
    [Column("experience_years")]
    public int ExperienceYears { get; set; }

    [MaxLength(200)]
    [Column("workplace")]
    public string? Workplace { get; set; }

    [Column("biography")]
    public string? Biography { get; set; }
}