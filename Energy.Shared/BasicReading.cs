using System.Text.Json.Serialization;

namespace Energy.Shared;

public record BasicReading
{
    [JsonPropertyName("K")]

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal KWh { get; init; }
    
    [JsonPropertyName("U")]
    public DateTime Utc { get; init; }
}   
