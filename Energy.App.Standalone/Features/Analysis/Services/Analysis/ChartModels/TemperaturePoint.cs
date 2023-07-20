namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;


public record TemperaturePoint
{
    public decimal TemperatureCelsiusUnmodified { get; init; }
    public decimal TemperatureCelsius { get; init; }
    public DateTime UtcTime { get; init; }
    public long DateTicks { get; init; }
    public string Summary { get; init; }
}
