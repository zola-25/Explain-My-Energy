using Energy.Shared;

namespace Energy.WeatherReadings.Interfaces
{
    public interface IWeatherDataService
    {
        Task<List<DailyWeatherReading>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null);
    }
}