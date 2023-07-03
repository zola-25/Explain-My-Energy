namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;


public class TemperaturePoint
{
    public double TemperatureCelsiusUnmodified { get; set; }
    public double TemperatureCelsius { get; set; }
    public DateTime ReadDate { get; set; }
    public long DateTicks { get; set; }
    public bool IsForecast { get; set; }
    public string Summary { get; set; }
}

public class TemperatureIconPoint
{
    public double TemperatureCelsiusUnmodified { get; set; }
    public double TemperatureCelsius { get; set; }
    public long DateTicks { get; set; }
    public string Summary { get; set; }
}