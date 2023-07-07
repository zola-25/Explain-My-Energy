using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Fluxor;

namespace Energy.App.Standalone.Features.Weather.Store
{
    public class WeatherEffects
    {
        private readonly IWeatherDataService _weatherDataService;
        private readonly IState<WeatherState> _weatherState;


        public WeatherEffects(IWeatherDataService weatherDataService, IState<WeatherState> weatherState)
        {
            _weatherDataService = weatherDataService;
            _weatherState = weatherState;
        }

        [EffectMethod]
        public async Task ReloadReadingsFromApi(InitiateWeatherReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<DailyWeatherReading> forecasts = await _weatherDataService.GetForOutCode(loadReadingsAction.OutCode);
            dispatcher.Dispatch(new StoreWeatherReloadedReadingsAction(forecasts));
        }

        [EffectMethod]
        public async Task UpdateReadingsFromApi(InitiateWeatherUpdateReadingsAction action, IDispatcher dispatcher)
        {
            var forecasts = (await _weatherDataService.GetForOutCode(action.OutCode, action.LatestHistorical, action.LatestReading)).OrderBy(c => c.UtcReadDate).ToList();
            dispatcher.Dispatch(new StoreWeatherUpdatedReadingsAction(forecasts));
        }

        [EffectMethod]
        public Task NotifyReady(StoreWeatherReloadedReadingsAction storeReadingsAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction());
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task NotifyReady(StoreWeatherUpdatedReadingsAction readingsAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction());
            return Task.CompletedTask;
        }
    }
}