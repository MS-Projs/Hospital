using DataAccess.Enums;
using DataAccess.Models;
using Newtonsoft.Json;

namespace Domain.Models.Common;

public class BasicViewModelLocalizedWithState : BasicViewModelLocalized
{
    [JsonProperty("isDeleted")]
    public EntityStatus Status { get; set; }

    public BasicViewModelLocalizedWithState() { }

    public BasicViewModelLocalizedWithState(EntityWithState model) : base(model)
        => Status = model.Status;
}