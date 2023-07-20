namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

public record DailyCostedReading
{
    public DateTime TariffAppliesFrom { get; init; }
    public decimal TariffDailyStandingChargePence { get; init; }

    public decimal PencePerKWh { get; init; }
    public DateTime UtcTime { get; init; }
    public decimal KWh { get; init; }
    public decimal ReadingTotalCostPounds { get; init; }
    public bool Forecast { get; init; }
    public bool IsFixedCostPerHour { get; init; }
}