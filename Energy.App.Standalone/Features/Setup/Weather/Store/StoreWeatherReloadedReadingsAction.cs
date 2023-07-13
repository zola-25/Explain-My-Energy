using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    public class StoreWeatherReloadedReadingsAction
    {
        public List<DailyWeatherRecord> ReloadedWeatherReadings { get; }

        public StoreWeatherReloadedReadingsAction(List<DailyWeatherRecord> reloadedWeatherReadings)
        {
            ReloadedWeatherReadings = reloadedWeatherReadings;
        }
    }
}