namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;

public class ChartDailyForecastReading
{
    public long DateTicks { get; set; }
    public decimal PencePerKWh { get; set; }
    public decimal Cost { get; set; }
    public decimal DailyStandingCharge { get; set; }
    public decimal KWh { get; set; }
    public DateTime TariffAppliesFrom { get; set; }
}