using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

public record CostedReading
{
    public DateTime TariffAppliesFrom { get; init; }
    public decimal TariffDailyStandingCharge { get; init; }
    public decimal TariffHalfHourlyStandingCharge { get; init; }
    public decimal TariffPencePerKWh { get; init; }

    public DateTime UtcTime { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal KWh { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public decimal CostPounds { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsFixedCostPerHour { get; init; }
}
