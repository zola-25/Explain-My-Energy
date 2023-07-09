namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;


public class TemperaturePoint
{
    public decimal TemperatureCelsiusUnmodified { get; set; }
    public decimal TemperatureCelsius { get; set; }
    public DateTime UtcTime { get; set; }
    public long DateTicks { get; set; }
    public bool IsForecast { get; set; }
    public string Summary { get; set; }
}

public class TemperatureIconPoint
{
    public decimal TemperatureCelsiusUnmodified { get; set; }
    public decimal TemperatureCelsius { get; set; }
    public long DateTicks { get; set; }
    public string Summary { get; set; }
}