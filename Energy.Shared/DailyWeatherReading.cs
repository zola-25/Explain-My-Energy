namespace Energy.Shared;

public class DailyWeatherReading
{
    public string OutCode { get; set; }

    public DateTime UtcTime { get; set; }
    public string Summary { get; set; }
    public string Icon { get; set; }

    public decimal TemperatureAverage { get; set; }

    public decimal? ApparentTemperatureMin { get; set; }
    public decimal? ApparentTemperatureMax { get; set; }
    public decimal? TemperatureMin { get; set; }
    public decimal? TemperatureMax { get; set; }
    public DateTime? Sunrise { get; set; }
    public DateTime? Sunset { get; set; }
    public decimal? WindSpeed { get; set; }

    public decimal? TotalRain { get; set; }
    public decimal? TotalSnowfall { get; set; }


    public decimal TemperatureFahrenheit
    {
        set => TemperatureAverage = (value - 32m) * 5m / 9m;
        get => (TemperatureAverage * (9m / 5m)) + 32m;
    }

    public bool IsClimateForecast { get; set; }

    public bool IsNearForecast { get; set; }

    public bool IsHistorical { get; set; }
    public bool IsRecentForecast { get; set; }


    public bool IsEstimate { get; set; }
    public DateTime Updated { get; set; }
}
