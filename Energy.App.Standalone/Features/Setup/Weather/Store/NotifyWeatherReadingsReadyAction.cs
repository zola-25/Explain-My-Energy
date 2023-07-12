namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    public class NotifyWeatherReadingsReadyAction
    {
        public int DaysUpdated { get; }

        public NotifyWeatherReadingsReadyAction(int daysUpdated)
        {
            DaysUpdated = daysUpdated;
        }
    }
}