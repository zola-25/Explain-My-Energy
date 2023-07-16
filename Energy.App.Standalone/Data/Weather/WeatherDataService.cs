using Energy.App.Standalone.Data.Weather.Interfaces;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.WeatherReadings;
using Energy.WeatherReadings.Interfaces;

namespace Energy.App.Standalone.Data.Weather
{
    public class WeatherDataService : IWeatherDataService
    {
        IMeteoWeatherDataService _meteoWeatherDataService;

        public WeatherDataService(IMeteoWeatherDataService meteoWeatherDataService)
        {
            _meteoWeatherDataService = meteoWeatherDataService;
        }

        public async Task<List<DailyWeatherRecord>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null)
        {
            var dailyWeatherReadings = await _meteoWeatherDataService.GetForOutCode(outCode, latestHistorical, latestReading);
            var results = dailyWeatherReadings.Select(x => Mapping.MapToDailyWeatherRecord(x)).ToList();
            return results;
        }
    }
}
