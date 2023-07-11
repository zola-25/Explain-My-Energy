namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects
{
    public record HourOfDayPriceState
    {
        public TimeSpan? HourOfDay { get; init; }

        public decimal PencePerKWh { get; init; }
    }
}
