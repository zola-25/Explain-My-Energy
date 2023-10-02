using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;

namespace Energy.App.Standalone.Data.Weather;

public static class Mapping
{
    public static DailyWeatherRecord MapToDailyWeatherRecord(DailyWeatherReading dailyWeatherReading)
    {
        return new DailyWeatherRecord
        {
            IsClimateForecast = dailyWeatherReading.IsClimateForecast,
            IsHistorical = dailyWeatherReading.IsHistorical,
            IsNearForecast = dailyWeatherReading.IsNearForecast,
            IsRecentForecast = dailyWeatherReading.IsRecentForecast,
            OutCode = dailyWeatherReading.OutCode,
            Summary = dailyWeatherReading.Summary,
            TemperatureAverage = dailyWeatherReading.TemperatureAverage,
            Updated = dailyWeatherReading.Updated,
            UtcTime = dailyWeatherReading.UtcTime
        };
    }
}
