using System.Collections.Immutable;
using Fluxor;
using Energy.Shared;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class DeleteHeatingForecastAction
{
    public MeterType HeatingMeterType { get; }

    public DeleteHeatingForecastAction(MeterType heatingMeterType)
    {
        HeatingMeterType = heatingMeterType;
    }

    [ReducerMethod]
    public static HeatingForecastState Reduce(HeatingForecastState state, DeleteHeatingForecastAction action)
    {
        return state with
        {
            C = 0,
            Gradient = 0,
            ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
            ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty,
            SavedCoefficients = false,
            CoefficientsUpdatedWithReadingDate = DateTime.MinValue,
            ForecastsUpdatedWithReadingDate = DateTime.MinValue,
            LoadingHeatingForecast = false,
            LatestReadingDate = DateTime.MinValue,
            HeatingMeterType = action.HeatingMeterType,
            LoadingCoefficients = false,

        };
    }
}
