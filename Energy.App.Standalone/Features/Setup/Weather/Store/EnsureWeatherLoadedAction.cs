using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.WeatherReadings.Interfaces;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Weather.Store
{

    public record EnsureWeatherLoadedAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<int> TaskCompletion { get; }

        public EnsureWeatherLoadedAction(bool forceReload, TaskCompletionSource<int> taskCompletion)
        {
            ForceReload = forceReload;
            TaskCompletion = taskCompletion;
        }

        [ReducerMethod(typeof(EnsureWeatherLoadedAction))]
        public static WeatherState Reduce(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }

        [ReducerMethod(typeof(NotifyWeatherLoadingFinished))]
        public static WeatherState Reduce(WeatherState state, NotifyWeatherLoadingFinished action)
        {
            if (action.DaysUpdated == 0) { 
                return state with
                {
                    Loading = false,
                };
            }
            return state with
            {
                Loading = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static WeatherState Reduce(WeatherState state, StoreWeatherReloadedReadingsAction action)
        {
            return state with
            {
                LastUpdated = DateTime.UtcNow,
                WeatherReadings = action.ReloadedWeatherReadings.ToImmutableList()
            };
        }

        [ReducerMethod]
        public static WeatherState Reduce(WeatherState state, StoreWeatherUpdatedReadingsAction action)
        {
            var firstUpdateDate = action.UpdatedWeatherReadings.First().UtcTime;

            return state with
            {
                LastUpdated = DateTime.UtcNow,
                WeatherReadings = state.WeatherReadings
                    .RemoveAll(c => c.UtcTime >= firstUpdateDate)
                    .AddRange(action.UpdatedWeatherReadings)
            };
        }

        private class Effect : Effect<EnsureWeatherLoadedAction>
        {
            private readonly IState<HouseholdState> _householdState;
            private readonly IState<WeatherState> _weatherState;
            private readonly IWeatherDataService _weatherDataService;

            public Effect(IState<HouseholdState> householdState,
                          IState<WeatherState> weatherState,
                          IWeatherDataService weatherDataService)
            {
                _householdState = householdState;
                _weatherState = weatherState;
                _weatherDataService = weatherDataService;
            }

            public override async Task HandleAsync(EnsureWeatherLoadedAction loadedAction, IDispatcher dispatcher)
            {
                var daysUpdated = 0;

                if (!_householdState.Value.Saved)
                {
                    dispatcher.Dispatch(new NotifyWeatherLoadingFinished(daysUpdated));
                    loadedAction.TaskCompletion.SetResult(daysUpdated);
                }

                string outCode = _householdState.Value.OutCodeCharacters;

                if (_weatherState.Value.WeatherReadings.eIsNullOrEmpty() || loadedAction.ForceReload)
                {
                    var results = await _weatherDataService.GetForOutCode(outCode);

                    dispatcher.Dispatch(new StoreWeatherReloadedReadingsAction(results));
                    daysUpdated = results.Count;
                }
                else
                {

                    var latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.
                        UtcTime;
                    var latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.
                        UtcTime;

                    if (_weatherState.Value.LastUpdated < DateTime.UtcNow.Date.AddDays(-1))
                    {

                        var results = await _weatherDataService.GetForOutCode(outCode, latestHistoricalReading, latestReading);

                        daysUpdated = results.Count;

                        dispatcher.Dispatch(new StoreWeatherUpdatedReadingsAction(results));
                    }
                }


                dispatcher.Dispatch(new NotifyWeatherLoadingFinished(daysUpdated));
                loadedAction.TaskCompletion.SetResult(daysUpdated);
            }
        }
    }
}