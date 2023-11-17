using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;

namespace Energy.App.Standalone.Data.Weather;

public static class Mapping
{
    public static DailyWeatherRecord MapToDailyWeatherRecord(DailyWeatherReading dailyWeatherReading)
    {
        return new DailyWeatherRecord
        {
            IsClimate = dailyWeatherReading.IsClimateForecast,
            IsHist = dailyWeatherReading.IsHistorical,
            IsNear = dailyWeatherReading.IsNearForecast,
            IsRecent = dailyWeatherReading.IsRecentForecast,
            Summary = dailyWeatherReading.Summary,
            TempAvg = dailyWeatherReading.TemperatureAverage,
            Utc = dailyWeatherReading.UtcTime
        };
    }
}
