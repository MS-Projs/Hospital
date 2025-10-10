using DataAccess.Schemas.Public;
using Domain.Models.Common;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class DocumentCategoryViewModel : BasicViewModelLocalizedWithState
{
    [JsonProperty("id")] 
    public long Id { get; set; }
    
    [JsonProperty("keyWord")]
    public string? KeyWord { get; set; }

    public DocumentCategoryViewModel(DocumentCategory documentCategory)
    {
        Id = documentCategory.Id;
        KeyWord = documentCategory.KeyWord;
        ValueEn = documentCategory.ValueEn;
        ValueRu = documentCategory.ValueRu;
        ValueUz = documentCategory.ValueUz;
        ValueUzl = documentCategory.ValueUzl;
        Status = documentCategory.Status;
    }
}
