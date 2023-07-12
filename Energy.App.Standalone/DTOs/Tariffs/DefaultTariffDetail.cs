using Energy.Shared;

namespace Energy.App.Standalone.DTOs.Tariffs
{
    public record DefaultTariffDetail
    {
        public ExampleTariffType ExampleTariffType { get; set; }

        public DateTime DateAppliesFrom { get; init; }
        public MeterType MeterType { get; init; }
        public decimal PencePerKWh { get; init; }
        public decimal DailyStandingChargePence { get; init; }
        public bool IsHourOfDayFixed { get; init; }
        public ICollection<DefaultHourOfDayPrice> DefaultHourOfDayPrices { get; init; }

    }
}
