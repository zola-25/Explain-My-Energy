namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    public record DailyWeatherRecord
    {
        public string OutCode { get; init; }

        public DateTime UtcTime { get; init; }
        public string Summary { get; init; }

        public decimal TemperatureAverage { get; init; }

        public bool IsClimateForecast { get; init; }

        public bool IsNearForecast { get; init; }

        public bool IsHistorical { get; init; }
        public bool IsRecentForecast { get; init; }

        public DateTime Updated { get; init; }
    }
}
