using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Weather.Store
{
    [PersistState]
    public record WeatherState
    {
        [property: JsonIgnore]
        public bool Loading { get; init; }
        public ImmutableList<DailyWeatherReading> WeatherReadings { get; init; }

        [property: JsonIgnore]
        public bool Updating { get; init; }
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
                Updating = false,
                WeatherReadings = ImmutableList<DailyWeatherReading>.Empty
            };
        }
    }

    public class WeatherReloadReadingsAction
    {
        public string OutCode { get; }

        public WeatherReloadReadingsAction(string outCode)
        {
            OutCode = outCode;
        }
    }

    public class WeatherUpdateReadingsAction
    {
        public string OutCode { get; }

        public DateTime? LatestReading { get; }
        public DateTime? LatestHistorical { get; }

        public WeatherUpdateReadingsAction(string outCode,
                                           DateTime? latestReading,
                                           DateTime? latestHistorical)
        {
            OutCode = outCode;
            LatestReading = latestReading;
            LatestHistorical = latestHistorical;
        }
    }

    public class WeatherReplaceAllReadingsAction
    {
        public List<DailyWeatherReading> WeatherReadings { get; }

        public WeatherReplaceAllReadingsAction(List<DailyWeatherReading> weatherReadings)
        {
            WeatherReadings = weatherReadings;
        }
    }

    public class WeatherStoreUpdatedReadingsAction
    {
        public List<DailyWeatherReading> WeatherReadings { get; }

        public WeatherStoreUpdatedReadingsAction(List<DailyWeatherReading> weatherReadings)
        {
            WeatherReadings = weatherReadings;
        }
    }



    public static class WeatherReducers
    {
        [ReducerMethod]
        public static WeatherState OnStoreReplaceReadingsReducer(WeatherState state, WeatherReplaceAllReadingsAction action)
        {
            return state with
            {
                Loading = false,
                WeatherReadings = action.WeatherReadings.ToImmutableList(),
            };
        }

        [ReducerMethod]
        public static WeatherState OnStoreUpdatedReadingsReducer(WeatherState state, WeatherStoreUpdatedReadingsAction action)
        {

            return state with
            {
                Updating = false,
                WeatherReadings = state.WeatherReadings.RemoveAll(c => c.UtcReadDate >= action.WeatherReadings.First().UtcReadDate).AddRange(action.WeatherReadings),
            };
        }




        [ReducerMethod(typeof(WeatherReloadReadingsAction))]
        public static WeatherState OnBeginLoadingReducer(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }

        [ReducerMethod(typeof(WeatherUpdateReadingsAction))]
        public static WeatherState OnBeginUpdatingReducer(WeatherState state)
        {
            return state with
            {
                Updating = true
            };
        }


    }

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
        public async Task LoadReadings(WeatherReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<DailyWeatherReading> forecasts = await _weatherDataService.GetForOutCode(loadReadingsAction.OutCode);
            dispatcher.Dispatch(new WeatherReplaceAllReadingsAction(forecasts));
        }

        [EffectMethod]
        public async Task UpdateReadings(WeatherUpdateReadingsAction action, IDispatcher dispatcher)
        {
            var forecasts = (await _weatherDataService.GetForOutCode(action.OutCode, action.LatestHistorical, action.LatestReading)).OrderBy(c => c.UtcReadDate).ToList();
            dispatcher.Dispatch(new WeatherStoreUpdatedReadingsAction(forecasts));
        }

        [EffectMethod]
        public Task NotifyReady(WeatherReplaceAllReadingsAction storeReadingsAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyWeatherReadingsLoadedAction());
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task NotifyReady(WeatherStoreUpdatedReadingsAction storeReadingsAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyWeatherReadingsLoadedAction());
            return Task.CompletedTask;
        }
    }

    public class NotifyWeatherReadingsLoadedAction
    {

    }

}
