using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;

public class DeleteElectricityHistoricalForecastAction
{
    [ReducerMethod(typeof(DeleteElectricityHistoricalForecastAction))]
    public static HistoricalForecastState Reduce(HistoricalForecastState state)
    {
        return state with
        {
            ElectricityForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
            ElectricityLastUpdate = DateTime.MinValue
        };
    }
}
