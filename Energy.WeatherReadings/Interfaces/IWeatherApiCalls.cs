using Energy.Shared;
using Energy.WeatherReadings.Models;

namespace Energy.WeatherReadings.Interfaces;

public interface IWeatherApiCalls
{
    Task LoadHistorical(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings);
    Task LoadForecast(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings);
    Task LoadClimate(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings);
}