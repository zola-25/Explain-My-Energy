using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

public record DailyCostedReading
{
    public DateTime TariffAppliesFrom { get; init; }
    public decimal TariffDailyStandingChargePence { get; init; }

    public decimal PencePerKWh { get; init; }
    public DateTime UtcTime { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal KWh { get; init; }
    
    public decimal ReadingTotalCostPounds { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFixedCostPerHour { get; init; }
}