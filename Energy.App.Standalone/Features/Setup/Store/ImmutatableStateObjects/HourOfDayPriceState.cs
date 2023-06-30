
namespace Energy.App.Standalone.Features.Setup.Store
{
    public record HourOfDayPriceState
    {
        public TimeSpan? HourOfDay { get; init; }

        public double PencePerKWh { get; init; }
    }
}
