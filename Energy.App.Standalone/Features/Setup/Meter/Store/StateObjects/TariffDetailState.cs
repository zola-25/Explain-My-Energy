using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;

public record TariffDetailState
{
    public Guid GlobalId { get; init; }

    public DateTime? DateAppliesFrom { get; init; }

    public decimal PencePerKWh { get; init; }

    public bool IsHourOfDayFixed { get; init; }

    public decimal DailyStandingChargePence { get; init; }

    public ImmutableList<HourOfDayPriceState> HourOfDayPrices { get; init; }
}
