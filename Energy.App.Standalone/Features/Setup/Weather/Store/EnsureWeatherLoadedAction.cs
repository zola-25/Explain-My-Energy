﻿using System.Collections.Immutable;
using Energy.App.Standalone.Data.Weather.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.WeatherReadings.Interfaces;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;


public record EnsureWeatherLoadedAction
{
    public bool ForceReload { get; }
    public TaskCompletionSource<(bool success, string message)> TaskCompletion { get; }

    public EnsureWeatherLoadedAction(bool forceReload, TaskCompletionSource<(bool success, string message)> taskCompletion = null)
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
    public static WeatherState NotifyWeatherLoadingFinishedReducer(WeatherState state)
    {
        return state with
        {
            Loading = false
        };
    }

    [ReducerMethod]
    public static WeatherState StoreWeatherReloadedReadingsActionReducer(WeatherState state, StoreWeatherReloadedReadingsAction action)
    {
        return state with
        {
            OutCode = action.OutCode,
            LastUpdated = DateTime.UtcNow,
            WReadings = action.ReloadedWeatherReadings.ToImmutableList()
        };
    }

    [ReducerMethod]
    public static WeatherState StoreWeatherUpdatedReadingsActionReducer(WeatherState state, StoreWeatherUpdatedReadingsAction action)
    {
        var firstUpdateDate = action.UpdatedWeatherReadings.First().Utc;

        return state with
        {
            LastUpdated = DateTime.UtcNow,
            WReadings = state.WReadings
                .RemoveAll(c => c.Utc >= firstUpdateDate)
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

        public override async Task HandleAsync(EnsureWeatherLoadedAction action, IDispatcher dispatcher)
        {

            if (!_householdState.Value.Saved || _householdState.Value.Invalid)
            {
                var message = "Household not saved or invalid";
                dispatcher.Dispatch(new NotifyWeatherLoadingFinished(false, message));
                action.TaskCompletion?.SetResult((false, message));
                return;
            }

            string outCode = _householdState.Value.OutCodeCharacters;


            if (_weatherState.Value.WReadings.eIsNullOrEmpty() || action.ForceReload)
            {

                var results = await _weatherDataService.GetForOutCode(outCode);

                dispatcher.Dispatch(new StoreWeatherReloadedReadingsAction(results, outCode));

                var message = $"Loaded {results.Count} days of weather data for {outCode}";
                dispatcher.Dispatch(new NotifyWeatherLoadingFinished(true, message));
                action.TaskCompletion?.SetResult((true, message));
                return;
            }
            else
            {

                var latestReading = _weatherState.Value.WReadings.FindLast(c => c.IsRecent)?.
                    Utc;
                var latestHistoricalReading = _weatherState.Value.WReadings.FindLast(c => c.IsHist)?.
                    Utc;

                if (_weatherState.Value.LastUpdated < DateTime.UtcNow.Date.AddDays(-1))
                {

                    var results = await _weatherDataService.GetForOutCode(outCode, latestHistoricalReading, latestReading);

                    if (results.Count == 0)
                    {
                        var noNewDataMessage = $"No new weather data available for {outCode}";
                        dispatcher.Dispatch(new NotifyWeatherLoadingFinished(true, noNewDataMessage));
                        action.TaskCompletion?.SetResult((true, noNewDataMessage));
                        return;
                    }

                    dispatcher.Dispatch(new StoreWeatherUpdatedReadingsAction(results, outCode));

                    var message = $"Updated {results.Count} new days of weather data for {outCode}";
                    dispatcher.Dispatch(new NotifyWeatherLoadingFinished(true, message));
                    action.TaskCompletion?.SetResult((true, message));

                    return;

                }
            }

            var noUpdateNeededMessage = $"Weather Data up-to-date for {outCode}";
            dispatcher.Dispatch(new NotifyWeatherLoadingFinished(true, noUpdateNeededMessage));
            action.TaskCompletion?.SetResult((true, noUpdateNeededMessage));
        }
    }
}