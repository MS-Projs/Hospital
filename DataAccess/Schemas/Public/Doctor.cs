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

    [Column("middle_name")]
    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("specialization")]
    public string Specialization { get; set; } = default!;

    [Range(0, 80)]
    [Column("experience_years")]
    public int ExperienceYears { get; set; }

    // eski bitta workplace endi legacy bo‘ladi
    [MaxLength(200)]
    [Column("workplace")]
    public string? Workplace { get; set; }

    [Column("work_phone")]
    [MaxLength(20)]
    public string? WorkPhone { get; set; }

    [Column("bio_keywords")]
    [MaxLength(500)]
    public string? BioKeywords { get; set; }

    [Column("academic_title")]
    [MaxLength(100)]
    public string? AcademicTitle { get; set; }

    [Column("category_rank")]
    [MaxLength(100)]
    public string? CategoryRank { get; set; }

    [Column("telegram_url")]
    [MaxLength(255)]
    public string? TelegramUrl { get; set; }

    [Column("social_consent")]
    public bool SocialConsent { get; set; } = false;

    [Column("biography")]
    public string? Biography { get; set; }
    
    public ICollection<DoctorEducation> Educations { get; set; } = new List<DoctorEducation>();
    public ICollection<DoctorSpecialization> Specializations { get; set; } = new List<DoctorSpecialization>();
    public ICollection<DoctorWorkplace> Workplaces { get; set; } = new List<DoctorWorkplace>();

}