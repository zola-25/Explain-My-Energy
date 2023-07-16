using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;
using Fluxor;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class DeleteHeatingForecastAction
    {
        [ReducerMethod(typeof(DeleteHeatingForecastAction))]
        public static HeatingForecastState Reduce(HeatingForecastState state)
        {
            return state with
            {
                C = 0,
                Gradient = 0,
                ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
                ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty,
                SavedCoefficients = false,
                CoefficientsLastUpdate = DateTime.MinValue,
                ForecastsLastUpdate = DateTime.MinValue,
            };
        }
    }
}
