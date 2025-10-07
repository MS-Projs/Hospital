using DataAccess.Schemas.Public;
using Domain.Models.Common;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class CertificateTypeViewModel : BasicViewModelLocalizedWithState
{
    [JsonProperty("id")] 
    public long Id { get; set; }
    
    [JsonProperty("KeyWord")]
    public string? KeyWord { get; set; }

    public CertificateTypeViewModel(CertificateType certificateType)
    {
        Id = certificateType.Id;
        KeyWord = certificateType.KeyWord;
        ValueEn = certificateType.ValueEn;
        ValueRu = certificateType.ValueRu;
        ValueUz = certificateType.ValueUz;
        ValueUzl = certificateType.ValueUzl;
    }
}