using System.Collections.ObjectModel;
using Blazored.LocalStorage;
using Energy.App.Blazor.Client.Services.Api.Interfaces;
using Energy.App.Blazor.Shared;

namespace Energy.App.Blazor.Client.StateContainers
{
    public class WeatherDataState
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IWeatherDataApi _weatherDataApi;

        public WeatherDataState(ILocalStorageService localStorageService, IWeatherDataApi weatherDataApi)
        {
            _localStorageService = localStorageService;
            _weatherDataApi = weatherDataApi;
        }

        public ReadOnlyCollection<DailyWeatherReading> WeatherReadings { get; private set; }

        public async Task LoadWeatherData()
        {
            var readings = await _weatherDataApi.GetWeatherData();
            WeatherReadings = readings.AsReadOnly();

            if (NotifyWeatherDataLoaded != null)
            {
                await NotifyWeatherDataLoaded.Invoke();
            }
        }

        public event Func<Task> NotifyWeatherDataLoaded;
    }
}
