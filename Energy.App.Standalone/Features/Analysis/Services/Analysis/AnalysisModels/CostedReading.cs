namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

public record CostedReading
{
    public DateTime TarrifAppliesFrom { get; init; }
    public decimal TariffDailyStandingCharge { get; init; }
    public decimal TariffHalfHourlyStandingCharge { get; init; }
    public decimal TariffPencePerKWh { get; init; }

    public DateTime UtcTime { get; init; }
    public decimal KWh { get; init; }
    public decimal CostPounds { get; init; }
    public bool IsForecast { get; init; }
    public bool IsFixedCostPerHour { get; init; }
}
