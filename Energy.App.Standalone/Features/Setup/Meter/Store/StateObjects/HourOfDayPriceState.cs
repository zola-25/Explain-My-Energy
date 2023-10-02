namespace Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;

public record HourOfDayPriceState
{
    public TimeSpan? HourOfDay { get; init; }

    public decimal PencePerKWh { get; init; }
}
