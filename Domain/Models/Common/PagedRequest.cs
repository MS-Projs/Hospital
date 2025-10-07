using System.Text.Json.Serialization;

namespace Domain.Models.Common;

public record PagedRequest
{
    public int PageNumber { get; init; } 
    public int PageSize { get; init; }
    [JsonIgnore]
    public bool All => PageNumber == 0 && PageSize == 0;
}