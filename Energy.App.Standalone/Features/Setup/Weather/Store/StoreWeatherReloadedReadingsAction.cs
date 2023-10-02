using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreWeatherReloadedReadingsAction
{
    public string OutCodeCharacters { get; }
    public List<DailyWeatherRecord> ReloadedWeatherReadings { get; }

    public StoreWeatherReloadedReadingsAction(List<DailyWeatherRecord> reloadedWeatherReadings, string outCodeCharacters)
    {
        ReloadedWeatherReadings = reloadedWeatherReadings;
        OutCodeCharacters = outCodeCharacters;
    }
}