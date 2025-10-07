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
        return new Doctor()
        {
            Id = src.Id,
            UserId = src.UserId,
            FullName = src.FullName,
            Specialization = src.Specialization.ToLower(),
            ExperienceYears = src.ExperienceYears,
            Workplace = src.Workplace,
            Biography = src.Biography
        };
    }
}