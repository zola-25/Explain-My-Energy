using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects
{
    public record TariffDetailState
    {
        public Guid GlobalId { get; init; }

        public DateTime? DateAppliesFrom { get; init; }

        public decimal PencePerKWh { get; init; }

        public bool IsHourOfDayFixed { get; init; }

        public decimal DailyStandingChargePence { get; init; }

        public ImmutableList<HourOfDayPriceState> HourOfDayPrices { get; init; }
    }
}
