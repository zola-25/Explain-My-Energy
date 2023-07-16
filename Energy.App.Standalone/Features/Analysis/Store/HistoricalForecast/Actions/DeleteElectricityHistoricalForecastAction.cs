using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions
{
    public class DeleteElectricityHistoricalForecastAction
    {
        [ReducerMethod]
        public static HistoricalForecastState Reduce(HistoricalForecastState state, DeleteElectricityHistoricalForecastAction action)
        {
            return state with
            {
                ElectricityForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
                ElectricityLastUpdate = DateTime.MinValue
            };
        }
    }
}
