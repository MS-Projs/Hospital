using DataAccess.Models;
using Newtonsoft.Json;

namespace Domain.Models.Common;

public class BasicViewModelLocalized : LocalizedViewModel
{
    [JsonProperty("id")]
    public long Id { get; set; }
    
    public BasicViewModelLocalized() : base() { }

    public BasicViewModelLocalized(EntityWithState model) : base(model)
    {
        Id = model.Id;
    }
}