namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public class CostedReading
    {
        public DateTime LocalTime { get; set; }
        public double PencePerKWh { get; set; }
        public double CostPence => KWh * PencePerKWh + HalfHourlyStandingCharge;
        public double DailyStandingChargePence { get; set; }
        public double KWh { get; set; }
        public double HalfHourlyStandingCharge { get; set; }
        public DateTime TariffAppliesFrom { get; set; }
        public TariffType TariffType { get; set; }
        public bool Forecast { get; set; }
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
