using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.WeatherReadings.Interfaces;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Weather.Store
{

    public record LoadWeatherAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<bool> TaskCompletion { get; }

        public LoadWeatherAction(bool forceReload, TaskCompletionSource<bool> taskCompletion)
        {
            ForceReload = forceReload;
            TaskCompletion = taskCompletion;
        }

        [ReducerMethod(typeof(LoadWeatherAction))]
        public static WeatherState Reduce(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }

        [ReducerMethod]
        public static WeatherState Reduce(WeatherState state, StoreWeatherReloadedReadingsAction action)
        {
            return state with
            {
                Loading = false,
                LastUpdated = DateTime.UtcNow,
                WeatherReadings = action.ReloadedWeatherReadings.ToImmutableList()
            };
        }

        [ReducerMethod]
        public static WeatherState Reduce(WeatherState state, StoreWeatherUpdatedReadingsAction action)
        {
            DateTime firstUpdateDate = action.UpdatedWeatherReadings.First().UtcTime;

            return state with
            {
                Loading = false,
                LastUpdated = DateTime.UtcNow,
                WeatherReadings = state.WeatherReadings
                    .RemoveAll(c => c.UtcTime >= firstUpdateDate)
                    .AddRange(action.UpdatedWeatherReadings)
            };
        }

        private class Effect : Effect<LoadWeatherAction>
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

            public override async Task HandleAsync(LoadWeatherAction action, IDispatcher dispatcher)
            {
                int daysUpdated = 0;

                if (!_householdState.Value.Saved)
                {
                    dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction(daysUpdated));
                    action.TaskCompletion.SetResult(true);
                }

                string outCode = _householdState.Value.OutCodeCharacters;

                if (_weatherState.Value.WeatherReadings.eIsNullOrEmpty() || action.ForceReload)
                {
                    List<Energy.Shared.DailyWeatherReading> results = await _weatherDataService.GetForOutCode(outCode);

                    dispatcher.Dispatch(new StoreWeatherReloadedReadingsAction(results));
                    daysUpdated = results.Count;
                }
                else
                {

                    DateTime? latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.
                        UtcTime;
                    DateTime? latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.
                        UtcTime;

                    if (_weatherState.Value.LastUpdated < DateTime.UtcNow.Date.AddDays(-1))
                    {

                        List<Energy.Shared.DailyWeatherReading> results = await _weatherDataService.GetForOutCode(outCode, latestHistoricalReading, latestReading);

                        daysUpdated = results.Count;

                        dispatcher.Dispatch(new StoreWeatherUpdatedReadingsAction(results));
                    }
                }


                dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction(daysUpdated));
                action.TaskCompletion.SetResult(true);
            }
        }
    }
}