using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;

public class StoreHistoricalForecastAction
{
    public MeterType MeterType { get; }
    public ImmutableList<DailyCostedReading> ForecastDailyCosts { get; }
    public DateTime LastUpdate { get; }

    public StoreHistoricalForecastAction(MeterType meterType, ImmutableList<DailyCostedReading> forecastDailyCosts, DateTime lastUpdate)
    {
        MeterType = meterType;
        ForecastDailyCosts = forecastDailyCosts;
        LastUpdate = lastUpdate;
    }

    [ReducerMethod]
    public static HistoricalForecastState StoreHistoricalForecastReducer(HistoricalForecastState state, StoreHistoricalForecastAction action)
    {
        return action.MeterType switch
        {
            MeterType.Electricity => state with
            {
                ElectricityForecastDailyCosts = action.ForecastDailyCosts,
                ElectricityLastUpdate = action.LastUpdate
            },
            MeterType.Gas => state with
            {
                GasForecastDailyCosts = action.ForecastDailyCosts,
                GasLastUpdate = action.LastUpdate
            },
            _ => throw new NotImplementedException(nameof(action.MeterType)),
        };
    }
}
