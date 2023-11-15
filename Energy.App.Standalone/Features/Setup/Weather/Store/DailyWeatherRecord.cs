using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public record DailyWeatherRecord
{
    public string OC { get; init; }

    public DateTime Utc { get; init; }
    public string Summary { get; init; }

    public decimal TempAvg { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsClimate { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsNear { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsHist { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsRecent { get; init; }

}
