namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;

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

    public long? HighlightStart { get; set; }
    public long? HighlightEnd { get; set; }
}
