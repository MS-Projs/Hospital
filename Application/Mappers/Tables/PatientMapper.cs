using DataAccess.Schemas.Public;
using Domain.Models.API.Requests;
using Mapster;

namespace Application.Mappers.Tables;

public class PatientMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpsertPatientRequest, Patient>().ConstructUsing(src => Map(src));
    }

    private Patient Map(UpsertPatientRequest src)
    {
        return new Patient()
        {
            Id = src.Id ?? 0,
            UserId = src.UserId,
            FullName = src.FullName,
            Age = src.Age,
            Gender = src.Gender.ToLower(),
            Address = src.Address,
            AdditionalNotes = src.AdditionalNotes
        };
    }
    
}