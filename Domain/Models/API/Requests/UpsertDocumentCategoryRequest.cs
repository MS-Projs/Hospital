using Domain.Models.Common;
using Newtonsoft.Json;

namespace Domain.Models.API.Requests;

public class UpsertDocumentCategoryRequest : BasicViewModelLocalizedWithState
{
    [JsonProperty("id")]
    public int? Id { get; set; }
    
    [JsonProperty("keyWord")]
    public string? KeyWord { get; set; }
}
