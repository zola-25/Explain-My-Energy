using System.Text.Json.Serialization;

namespace Energy.Shared;

public record BasicReading
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal KWh { get; init; }
    public DateTime UtcTime { get; init; }
}
