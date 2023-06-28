namespace Energy.Shared
{
    public class DailyWeatherReading 
    {
        public string OutCode { get; set; }

        public DateTime ReadDate { get; set; }
        public string Summary { get; set; }
        public string Icon { get; set; }

        public double TemperatureAverage { get; set; }

        public double? ApparentTemperatureMin { get; set; }
        public double? ApparentTemperatureMax { get; set; }
        public double? TemperatureMin { get; set; }
        public double? TemperatureMax { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public double? WindSpeed { get; set; }

        public double? TotalRain { get; set; }
        public double? TotalSnowfall { get; set; }


        public double TemperatureFahrenheit
        {
            set => TemperatureAverage = (value - 32d) * 5d / 9d;
            get => (TemperatureAverage * (9d / 5d)) + 32d;
        }

        public bool IsClimateForecast { get; set; }

        public bool IsNearForecast { get; set; }

        public bool IsHistorical { get; set; }
        public bool IsRecentForecast { get; set; }


        public bool IsEstimate { get; set; }
        public DateTime Updated { get; set; }
    }
}
