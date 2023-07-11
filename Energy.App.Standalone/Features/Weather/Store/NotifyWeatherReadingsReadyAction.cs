namespace Energy.App.Standalone.Features.Weather.Store
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