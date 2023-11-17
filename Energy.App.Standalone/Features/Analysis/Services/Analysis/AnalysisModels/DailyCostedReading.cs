using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

public record DailyCostedReading
{
    [JsonPropertyName("TFrom")]
    public DateTime TariffAppliesFrom { get; init; }
    
    [JsonPropertyName("TDailyStP")]
    public decimal TariffDailyStandingChargePence { get; init; }



    [JsonPropertyName("PpKWh")]

    public decimal PencePerKWh { get; init; }
    
    [JsonPropertyName("Utc")]
    public DateTime UtcTime { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal KWh { get; init; }
    
    [JsonPropertyName("CostGBP")]
    public decimal ReadingTotalCostPounds { get; init; }
    
    [JsonPropertyName("Fixed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFixedCostPerHour { get; init; }
}