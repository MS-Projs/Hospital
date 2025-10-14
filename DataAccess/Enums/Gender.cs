using Newtonsoft.Json;

namespace DataAccess.Enums;

public enum Gender
{
    [JsonProperty("male")]
    Male = 1,
    
    [JsonProperty("female")]
    Female = 2,
}