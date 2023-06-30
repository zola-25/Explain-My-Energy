namespace Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects
{
    public record HourOfDayPriceState
    {
        public TimeSpan? HourOfDay { get; init; }

        public double PencePerKWh { get; init; }
    }
}
