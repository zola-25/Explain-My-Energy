namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public record CostedReading
    {
        public DateTime TariffAppliesFrom { get; init; }
        public decimal TariffDailyStandingChargePence { get; init; }
        public decimal TariffHalfHourlyStandingChargePence { get; init; }
        public decimal TariffHalfHourlyPencePerKWh { get; init; }

        public DateTime UtcTime { get; init; }
        public decimal KWh { get; init; }
        public decimal ReadingTotalCostPence { get; init; } 
        public bool Forecast { get; init; }
    }
}
