using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreWeatherReloadedReadingsAction
{
    public List<DailyWeatherRecord> ReloadedWeatherReadings { get; }
    public string OutCode { get; }
    public DateTime LastUpdated { get; }

    public StoreWeatherReloadedReadingsAction(List<DailyWeatherRecord> reloadedWeatherReadings, string outCode, DateTime lastUpdated)
    {
        ReloadedWeatherReadings = reloadedWeatherReadings;
        OutCode = outCode;
        LastUpdated = lastUpdated;
    }
}