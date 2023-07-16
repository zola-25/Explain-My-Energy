namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public class MeterChartProfile
    {
        public bool ShowCost { get; set; }
        public Guid GlobalId { get; set; }
        public long ProfileStart { get; set; }
        public long ProfileEnd { get; set; }
        public long LatestReading { get; set; }
        public long MostRecentWeekStart { get; set; }
        public long OneMonthInTheFuture { get; set; }
        public List<ChartReading> ChartReadings { get; set; }
        public List<ChartDailyForecastReading> ChartDailyForecastReadings { get; set; }

        public long? HighlightStart {get; set; }
        public long? HighlightEnd {get; set; }

    }

    public class MeterChartData
    {
        public MeterChartProfile MeterChartProfile { get; set; }
        public List<TemperaturePoint> TemperaturePoints { get; set; }
    }
}
