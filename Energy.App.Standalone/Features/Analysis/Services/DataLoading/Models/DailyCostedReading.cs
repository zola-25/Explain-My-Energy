namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

public record DailyCostedReading
{
    public DateTime TariffAppliesFrom { get; init; }
    public decimal TariffDailyStandingChargePence { get; init; }

    public DateTime UtcTime { get; init; }
    public decimal KWh { get; init; }
    public decimal ReadingTotalCostPence { get; init; } 
    public bool Forecast { get; init; }
}