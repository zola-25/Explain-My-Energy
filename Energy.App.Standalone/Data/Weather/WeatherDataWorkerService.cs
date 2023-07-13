using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using SpawnDev.BlazorJS.WebWorkers;

namespace Energy.App.Standalone.Data.Weather
{
    public class WeatherDataWorkerService : IWeatherDataWorkerService
    {
        WebWorkerService _webWorkerService;

        public WeatherDataWorkerService(WebWorkerService webWorkerService)
        {
            _webWorkerService = webWorkerService;
        }

        public async Task<List<DailyWeatherRecord>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null)
        {

            using var webWorker = await _webWorkerService.GetWebWorker();
            var weatherDataWorkerService = webWorker.GetService<IWeatherDataService>();

            var dailyWeatherReadings = await weatherDataWorkerService.GetForOutCode(outCode, latestHistorical, latestReading);
            var results = dailyWeatherReadings.Select(x => MapToDailyWeatherRecord(x)).ToList();
            return results;
        }

        // Function to Map DailyWeatherReading to DailyWeatherRecord
        private DailyWeatherRecord MapToDailyWeatherRecord(DailyWeatherReading dailyWeatherReading)
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
}
