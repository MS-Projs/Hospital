using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Enums;
using DataAccess.Schemas.Public;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class DoctorViewModel
{
    public long Id { get; set; }
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
    public bool SocialConsent { get; set; }

    public EntityStatus Status { get; set; }

    public DoctorViewModel() { }

    public DoctorViewModel(Doctor doctor)
    {
        Id = doctor.Id;
        UserId = doctor.UserId;
        FullName = doctor.FullName;
        Specialization = doctor.Specialization;
        Workplace = doctor.Workplace;
        Biography = doctor.Biography;
        MiddleName = doctor.MiddleName;
        WorkPhone = doctor.WorkPhone;
        BioKeywords = doctor.BioKeywords;
        AcademicTitle = doctor.AcademicTitle;
        CategoryRank = doctor.CategoryRank;
        TelegramUrl = doctor.TelegramUrl;
        SocialConsent = doctor.SocialConsent;
        Status = doctor.Status;
    }
}