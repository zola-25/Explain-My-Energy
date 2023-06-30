namespace Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects
{
    public record TariffDetailState
    {
        public Guid GlobalId { get; init; }

        public DateTime? DateAppliesFrom { get; init; }

        public double PencePerKWh { get; init; }

        public bool IsHourOfDayFixed { get; init; }

        public double DailyStandingChargePence { get; init; }

        public List<HourOfDayPriceState> HourOfDayPrices { get; init; }
    }
}
