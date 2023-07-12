namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    public class NotifyWeatherLoadingFinished
    {
        public int DaysUpdated { get; }

        public NotifyWeatherLoadingFinished(int daysUpdated)
        {
            DaysUpdated = daysUpdated;
        }
    }
}