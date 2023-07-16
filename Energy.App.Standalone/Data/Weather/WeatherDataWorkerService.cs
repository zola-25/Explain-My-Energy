//using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
//using Energy.App.Standalone.Data.Weather.Interfaces;
//using Energy.App.Standalone.Features.Setup.Weather.Store;
//using Energy.Shared;
//using Energy.WeatherReadings.Interfaces;
//using SpawnDev.BlazorJS.WebWorkers;

//namespace Energy.App.Standalone.Data.Weather
//{
//    public class WeatherDataWorkerService : IWeatherDataWorkerService
//    {
//        private readonly WebWorkerService _webWorkerService;

//        public WeatherDataWorkerService(WebWorkerService webWorkerService)
//        {
//            _webWorkerService = webWorkerService;
//        }

//        public async Task<List<DailyWeatherRecord>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null)
//        {

//            using var webWorker = await _webWorkerService.GetWebWorker();
//            var weatherDataWorkerService = webWorker.GetService<IMeteoWeatherDataService>();

//            var dailyWeatherReadings = await weatherDataWorkerService.GetForOutCode(outCode, latestHistorical, latestReading);
//            var results = dailyWeatherReadings.Select(x => Mapping.MapToDailyWeatherRecord(x)).ToList();
//            return results;
//        }


//    }
//}
