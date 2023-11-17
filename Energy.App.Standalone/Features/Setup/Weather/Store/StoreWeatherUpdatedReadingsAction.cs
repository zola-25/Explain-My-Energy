using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreWeatherUpdatedReadingsAction
{
    public List<DailyWeatherRecord> UpdatedWeatherReadings { get; }

    public string OutCode { get; } = string.Empty;

    public StoreWeatherUpdatedReadingsAction(List<DailyWeatherRecord> updatedWeatherReadings, string outCode)
    {
        UpdatedWeatherReadings = updatedWeatherReadings;
        OutCode = outCode;
    }
}