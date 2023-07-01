namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

public class ForecastAnalysis
{
    public string DateText => $"{Start.eDateToMinimal()} - {End.eDateToMinimal()}";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public double ForecastCostPence { get; set; }
    public double ForecastConsumption { get; set; }
    public int NumberOfDays { get; set; }
    public double ForecastCo2 { get; set; }
    public TemperatureRange TemperatureRange { get; set; }
}
