using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions
{
    public class DeleteGasHistoricalForecastAction
    {
        [ReducerMethod(typeof(DeleteGasHistoricalForecastAction))]
        public static HistoricalForecastState Reduce(HistoricalForecastState state)
        {
            return state with
            {
                GasForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
                GasLastUpdate = DateTime.MinValue
            };
        }
    }
}
