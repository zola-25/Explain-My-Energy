using Fluxor;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Energy.App.Standalone.Features.Weather.Store
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public static class WeatherReducers
    {
        [ReducerMethod]
        public static WeatherState OnStoreReloadedReadingsReducer(WeatherState state, StoreWeatherReloadedReadingsAction action)
        {
            return state with
            {
                Loading = false,
                WeatherReadings = action.ReloadedWeatherReadings.ToImmutableList(),
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static WeatherState OnStoreUpdatedReadingsReducer(WeatherState state, StoreWeatherUpdatedReadingsAction action)
        {
            var firstUpdateDate = action.UpdatedWeatherReadings.First().UtcTime;
            return state with
            {
                Updating = false,
                WeatherReadings = state.WeatherReadings
                    .RemoveAll(c => c.UtcTime >= firstUpdateDate)
                    .AddRange(action.UpdatedWeatherReadings),
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod(typeof(InitiateWeatherReloadReadingsAction))]
        public static WeatherState OnBeginReloadReadingLoadingReducer(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }

        [ReducerMethod(typeof(InitiateWeatherUpdateReadingsAction))]
        public static WeatherState OnBeginUpdateReadingsReducer(WeatherState state)
        {
            return state with
            {
                Updating = true
            };
        }


    }
}