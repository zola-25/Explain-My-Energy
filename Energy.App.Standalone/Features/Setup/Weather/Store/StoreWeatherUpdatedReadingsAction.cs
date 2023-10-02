using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreWeatherUpdatedReadingsAction
{
    public List<DailyWeatherRecord> UpdatedWeatherReadings { get; }

    public StoreWeatherUpdatedReadingsAction(List<DailyWeatherRecord> updatedWeatherReadings)
    {
        UpdatedWeatherReadings = updatedWeatherReadings;
    }
}