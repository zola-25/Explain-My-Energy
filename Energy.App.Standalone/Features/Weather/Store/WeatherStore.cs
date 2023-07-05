using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Weather.Store
{
    [PersistState]
    public record WeatherState
    {
        public bool Loading { get; init; }
        public ImmutableList<DailyWeatherReading> WeatherReadings { get; init; }
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
                WeatherReadings = ImmutableList<DailyWeatherReading>.Empty
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
                Loading = false,
                WeatherReadings = action.WeatherReadings.ToImmutableList(),
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

        [EffectMethod]
        public Task NotifyReady(WeatherStoreReadingsAction storeReadingsAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyWeatherReadingsLoadedAction());
            return Task.CompletedTask;
        }
    }

    public class NotifyWeatherReadingsLoadedAction
    {
        
    }

}
