using Energy.Shared;

namespace Energy.App.Standalone.Models.Tariffs
{
    public record DefaultTariffDetail
    {
        public string Name { get; init; }
        public DateTime DateAppliesFrom { get; init; }
        public MeterType MeterType { get; init; }
        public double PencePerKWh { get; init; }
        public double DailyStandingChargePence { get; init; }
        public bool IsHourOfDayFixed { get; init; }
        public ICollection<DefaultHourOfDayPrice> DefaultHourOfDayPrices { get; init; }

    }
}
