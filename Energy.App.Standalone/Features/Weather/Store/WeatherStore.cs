using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Weather.Store
{
    [PersistState, PriorityLoad]
    public record WeatherState
    {
        public bool Loading { get; init; }
        public List<DailyWeatherReading> WeatherReadings { get; init; }
    }

    public class WeatherFeature : Feature<WeatherState>
    {
        public override string GetName()
        {
            return nameof(WeatherFeature);
        }

        protected override WeatherState GetInitialState()
        {
            return new WeatherState
            {
                Loading = false,
                WeatherReadings = new List<DailyWeatherReading>()
            };
        }
    }

    public class WeatherLoadReadingsAction
    {
        public string OutCode { get; }

        public WeatherLoadReadingsAction(string outCode)
        {
            OutCode = outCode;
        }
    }

    public class WeatherStoreReadingsAction
    {
        public List<DailyWeatherReading> WeatherReadings { get; }

        public WeatherStoreReadingsAction(List<DailyWeatherReading> weatherReadings)
        {
            WeatherReadings = weatherReadings;
        }
    }



    public static class WeatherReducers
    {
        [ReducerMethod]
        public static WeatherState OnStoreReadingsReducer(WeatherState state, WeatherStoreReadingsAction action)
        {
            return state with
            {
                WeatherReadings = action.WeatherReadings,
                Loading = false
            };
        }


        [ReducerMethod(typeof(WeatherLoadReadingsAction))]
        public static WeatherState OnBeginLoadingReducer(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }


    }

    public class WeatherEffects
    {
        private readonly IWeatherDataService _weatherDataService;

        public WeatherEffects(IWeatherDataService weatherDataService)
        {
            _weatherDataService = weatherDataService;
        }

        [EffectMethod]
        public async Task LoadReadings(WeatherLoadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<DailyWeatherReading> forecasts = await _weatherDataService.GetForOutCode(loadReadingsAction.OutCode);
            dispatcher.Dispatch(new WeatherStoreReadingsAction(forecasts));
        }
    }

}
