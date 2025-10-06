using DataAccess.Models;
using Newtonsoft.Json;

namespace Domain.Models.Common;

public class LocalizedViewModel
{
    [JsonProperty("valueRu")]
    public string? ValueRu { get; set; }

    [JsonProperty("valueUz")]
    public string? ValueUz { get; set; }

    [JsonProperty("valueUzl")]
    public string? ValueUzl { get; set; }

    [JsonProperty("valueEn")]
    public string? ValueEn { get; set; }

    public LocalizedViewModel() { }

    public LocalizedViewModel(EntityWithState model)
    {
        ValueRu = model.ValueRu;
        ValueUz = model.ValueUz;
        ValueUzl = model.ValueUzl;
        ValueEn = model.ValueEn;
    }
}