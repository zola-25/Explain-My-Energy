namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public record CostedReading
    {
        public DateTime LocalTime { get; init; }
        public decimal PencePerKWh { get; init; }
        public decimal CostPence { get; init; } 
        public decimal DailyStandingChargePence { get; init; }
        public decimal KWh { get; init; }
        public decimal HalfHourlyStandingChargePence { get; init; }
        public DateTime TariffAppliesFrom { get; init; }
        public TariffType TariffType { get; init; }
        public bool Forecast { get; init; }
    }

    public class ChartReading
    {
        public long DateTicks { get; set; }
        public double PencePerKWh { get; set; }
        public double Cost { get; set; }
        public double DailyStandingCharge { get; set; }
        public double KWh { get; set; }
        public double HalfHourlyStandingCharge { get; set; }
        public DateTime TariffAppliesFrom { get; set; }
        public string TariffType { get; set; }
        public bool IsForecast { get; set; }
    }
}
