using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using Fluxor.Persist.Storage;
using System.Text.Json.Serialization;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;

[FeatureState(Name = nameof(HeatingForecastState))]
[PersistState]
public record HeatingForecastState
{

    [property: JsonIgnore]
    public ImmutableList<DailyCostedReading> ForecastDailyCosts { get; init; }

    [property: JsonIgnore]
    public ImmutableList<TemperaturePoint> ForecastWeatherReadings { get; init; }

    public DateTime CoefficientsUpdatedWithReadingDate { get; init; }
    public DateTime ForecastsUpdatedWithReadingDate { get; init; }

    public bool SavedCoefficients { get; init; }
    public decimal Gradient { get; init; }
    public decimal C { get; init; }

    public bool LoadingHeatingForecast { get; init; }

    public MeterType HeatingMeterType { get; init; }
    public DateTime LatestReadingDate { get; init; }
    public bool LoadingCoefficients { get; init; }

    public HeatingForecastState()
    {
        ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
        ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty;
        CoefficientsUpdatedWithReadingDate = DateTime.MinValue;
        ForecastsUpdatedWithReadingDate = DateTime.MinValue;
        LatestReadingDate = DateTime.MinValue;
        SavedCoefficients = false;
        Gradient = 0;
        C = 0;
        LoadingHeatingForecast = false;
        LoadingCoefficients = false;
        HeatingMeterType = MeterType.Gas;
    }
}



















