using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

public record HistoricalAnalysis
{
    public string DateRangeText => $"{Start.eDateToMinimal()} - {End.eDateToMinimal()}";

    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public DateTime? LatestReading { get; init; }

    public decimal PeriodCo2 { get; init; }

    public decimal PeriodConsumptionKWh { get; init; }
    public decimal PeriodCostPounds { get; init; }

    public TemperatureRange TemperatureRange { get; init; }
    public bool HasData { get; init; }
}

public record TemperatureRange
{
    public int AverageTemp { get; init; }
    public int LowDailyTemp { get; init; }
    public int HighDailyTemp { get; init; }

    public string Symbol => "°C";
    public string TemperatureText => $"{LowDailyTemp} - {HighDailyTemp}{Symbol}";
    public string TemperatureColourStyle => $"background-image: linear-gradient(to right, {LowDailyTemp.eTempToHexColour()}, {AverageTemp.eTempToHexColour()} 50%, {HighDailyTemp.eTempToHexColour()});";

}