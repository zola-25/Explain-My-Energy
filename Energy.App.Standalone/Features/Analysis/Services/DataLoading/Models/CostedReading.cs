namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public record CostedReading
    {
        public decimal TariffDailyStandingChargePence { get; init; }
        public decimal TariffHalfHourlyStandingChargePence { get; init; }
        public decimal TariffHalfHourlyPencePerKWh { get; init; }
        public DateTime TariffAppliesFrom { get; init; }

        public DateTime UtcTime { get; init; }
        public decimal ReadingTotalCostPence { get; init; } 
        public decimal KWh { get; init; }
        public bool Forecast { get; init; }
    }

    public class ChartReading
    {
        public long DateTicks { get; set; }
        public decimal PencePerKWh { get; set; }
        public decimal Cost { get; set; }
        public decimal DailyStandingCharge { get; set; }
        public decimal KWh { get; set; }
        public decimal HalfHourlyStandingCharge { get; set; }
        public DateTime TariffAppliesFrom { get; set; }
        public string TariffType { get; set; }
        public bool IsForecast { get; set; }
    }
}
