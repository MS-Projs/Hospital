using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpsertDoctorRequest
{
    public long Id { get; set; } = 0; // 0 = yangi, >0 = update
    public long UserId { get; set; }
    
    public string FullName { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public string? Workplace { get; set; }
    public string? Biography { get; set; }

    // Yangi maydonlar
    public string? MiddleName { get; set; }
    public string? WorkPhone { get; set; }
    public string? BioKeywords { get; set; }
    public string? AcademicTitle { get; set; }
    public string? CategoryRank { get; set; }
    public string? TelegramUrl { get; set; }
    public bool SocialConsent { get; set; } = false;

    // Relations
    public List<DoctorWorkplaceRequest>? Workplaces { get; set; }
    public List<DoctorEducationRequest>? Educations { get; set; }
    public List<DoctorSpecializationRequest>? Specializations { get; set; }
}