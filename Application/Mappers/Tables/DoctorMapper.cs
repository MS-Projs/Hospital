using DataAccess.Schemas.Public;
using Domain.Models.API.Requests;
using Mapster;

namespace Application.Mappers.Tables;
public class DoctorMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpsertDoctorRequest, Doctor>().ConstructUsing(src => Map(src));
    }

    private Doctor Map(UpsertDoctorRequest src)
    {
        var doctor = new Doctor()
        {
            Id = src.Id,
            UserId = src.UserId,
            FullName = src.FullName?.Trim(),
            Specialization = src.Specialization?.Trim().ToLower(),
            Workplace = src.Workplace?.Trim(),
            Biography = src.Biography?.Trim(),

            // Yangi maydonlar
            MiddleName = src.MiddleName?.Trim(),
            WorkPhone = src.WorkPhone?.Trim(),
            AcademicTitle = src.AcademicTitle?.Trim(),
            CategoryRank = src.CategoryRank?.Trim(),
            TelegramUrl = src.TelegramUrl?.Trim(),
            SocialConsent = src.SocialConsent
        };

        // Workplaces, Educations va Specializations mapping (agar listlar bo‘lsa)
        if (src.Workplaces != null)
        {
            doctor.Workplaces = src.Workplaces
                .Select(w => new DoctorWorkplace
                {
                    ClinicName = w.ClinicName,
                    ClinicAddress = w.ClinicAddress
                }).ToList();
        }

        if (src.Educations != null)
        {
            doctor.Educations = src.Educations
                .Select(e => new DoctorEducation
                {
                    University = e.University, 
                    DegreeType =  e.DegreeType,
                    Country = e.Country,
                    Specialization = e.Specialization,
                    GraduationYear =  e.GraduationYear
                }).ToList();
        }

        if (src.Specializations != null)
        {
            doctor.Specializations = src.Specializations
                .Select(s => new DoctorSpecialization
                {
                    Name = s.Name
                }).ToList();
        }

        return doctor;
    }
}