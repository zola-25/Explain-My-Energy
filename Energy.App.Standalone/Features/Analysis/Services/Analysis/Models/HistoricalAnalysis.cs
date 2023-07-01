namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

public class HistoricalAnalysis
{
    public string DateRangeText => $"{Start.eDateToMinimal()} - {End.eDateToMinimal()}";

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime? LatestReading { get; set; }

    public double PeriodCo2 { get; set; }

    public double PeriodConsumptionKWh { get; set; }
    public double PeriodCost { get; set; }

    public TemperatureRange TemperatureRange { get; set; }
    public bool HasData { get; set; }
}

public class TemperatureRange
{
    public int AverageTemp { get; set; }
    public int LowDailyTemp { get; set; }
    public int HighDailyTemp { get; set; }

    public string Symbol => "°C";
    public string TemperatureText => $"{LowDailyTemp} - {HighDailyTemp}{Symbol}";
    public string TemperatureColourStyle => $"background-image: linear-gradient(to right, {LowDailyTemp.eTempToHexColour()}, {AverageTemp.eTempToHexColour()} 50%, {HighDailyTemp.eTempToHexColour()});";

}