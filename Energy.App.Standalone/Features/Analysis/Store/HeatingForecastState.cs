using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Analysis.Store;

public record HeatingForecastState
{
    public ImmutableList<DailyCostedReading> ForecastDailyCosts { get; init; }
    public ImmutableList<TemperaturePoint> ForecastWeatherReadings { get; init; }

}

public class HeatingForecastFeature : Feature<HeatingForecastState>
{
    public override string GetName() => nameof(HeatingForecastFeature);

    protected override HeatingForecastState GetInitialState()
    {
        return new HeatingForecastState
        {
            ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
            ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty
        };
    }
}

public static class HeatingForecastReducers
{
    [ReducerMethod]
    public static HeatingForecastState ReduceLoadHeatingForecastAction(HeatingForecastState state,
        SetHeatingForecastAction action)
    {
        return state with
        {
            ForecastDailyCosts = action.ForecastDailyReadings,
            ForecastWeatherReadings = action.TemperatureIconPoints
        };
    }
}

public class SetHeatingForecastAction
{
    public ImmutableList<DailyCostedReading> ForecastDailyReadings { get; }
    public ImmutableList<TemperaturePoint> TemperatureIconPoints { get; }

    public SetHeatingForecastAction(ImmutableList<DailyCostedReading> forecastDailyReadings, ImmutableList<TemperaturePoint> temperatureIconPoints)
    {
        this.ForecastDailyReadings = forecastDailyReadings;
        TemperatureIconPoints = temperatureIconPoints;
    }
}

public class LoadHeatingForecastAction
{
    public decimal DegreeDifference { get; }

    public LoadHeatingForecastAction(decimal degreeDifference)
    {
        DegreeDifference = degreeDifference;
    }
}


public class HeatingForecastEffects
{
    private readonly IForecastGenerator _forecastGenerator;
    private readonly IState<WeatherState> _weatherState;
    private readonly IState<LinearCoefficientsState> _linearCoefficientsState;
    private readonly ICostCalculator _costCalculator;
    private readonly IState<ElectricityTariffsState> _electricityTariffsState;
    private readonly IState<GasTariffsState> _gasTariffsState;
    private readonly IState<HouseholdState> _householdState;

    public HeatingForecastEffects(IForecastGenerator forecastGenerator,
        IState<WeatherState> weatherState,
        IState<LinearCoefficientsState> linearCoefficientsState,
        ICostCalculator costCalculator,
        IState<ElectricityTariffsState> electricityTariffsState,
        IState<GasTariffsState> gasTariffsState,
        IState<HouseholdState> householdState)
    {
        _forecastGenerator = forecastGenerator;
        _weatherState = weatherState;
        _linearCoefficientsState = linearCoefficientsState;
        _costCalculator = costCalculator;
        _electricityTariffsState = electricityTariffsState;
        _gasTariffsState = gasTariffsState;
        _householdState = householdState;
    }

    [EffectMethod]
    public async Task HandleElectricityDegreeDifferenceChangedAction(
        ElectricityAnalysisOptionsSetDegreeDifferenceAction action,
        IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadHeatingForecastAction(action.DegreeDifference));
    }

    [EffectMethod]
    public async Task HandleGasDegreeDifferenceChangedAction(GasAnalysisOptionsSetDegreeDifferenceAction action,
        IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadHeatingForecastAction(action.DegreeDifference));
    }

    [EffectMethod]
    public async Task HandleLoadHeatingForecastAction(LoadHeatingForecastAction action, IDispatcher dispatcher)
    {
        var meterType = _householdState.Value.PrimaryHeatSource;
        var tariffs = meterType == MeterType.Electricity
            ? _electricityTariffsState.Value.TariffDetails
            : _gasTariffsState.Value.TariffDetails;

        var recentWeatherReadings = _weatherState.Value.WeatherReadings.Where
            (
                c => c.UtcTime >= DateTime.UtcNow.AddMonths(-2).
                    Date
            ).
            ToList();
        var basicReadingsForecast = _forecastGenerator.GetBasicReadingsForecast
            (action.DegreeDifference, _linearCoefficientsState.Value, recentWeatherReadings);

        var costedReadingsForecast = _costCalculator.GetCostReadings(basicReadingsForecast, tariffs);
        var dailyAggregatedCostedReadings = costedReadingsForecast.GroupBy(c => c.UtcTime.Date).
            Select
            (
                c => new DailyCostedReading()
                {
                    UtcTime = c.Key,
                    ReadingTotalCostPence = c.Sum(d => d.ReadingTotalCostPence),
                    TariffAppliesFrom = c.First().
                        TariffAppliesFrom,
                    TariffDailyStandingChargePence = c.First().
                        TariffDailyStandingChargePence,
                    KWh = c.Sum(d => d.KWh),
                    Forecast = true,
                }
            ).
            ToImmutableList();

        var temperatureIconPoints = recentWeatherReadings.Select
            (
                c => new TemperaturePoint()
                {
                    UtcTime = c.UtcTime,
                    DateTicks = c.UtcTime.Ticks,
                    TemperatureCelsius = c.TemperatureAverage + action.DegreeDifference,
                    TemperatureCelsiusUnmodified = c.TemperatureAverage,
                    Summary = c.Summary ?? String.Empty
                }
            ).
            ToImmutableList();

        dispatcher.Dispatch(new SetHeatingForecastAction(dailyAggregatedCostedReadings, temperatureIconPoints));
        dispatcher.Dispatch(new NotifyHeatingForecastReadyAction(meterType, action.DegreeDifference));
    }
}

public class NotifyHeatingForecastReadyAction
{
    MeterType MeterType { get; }
    public decimal DegreeDifference { get; }

    public NotifyHeatingForecastReadyAction(MeterType meterType, decimal degreeDifference)
    {
        MeterType = meterType;
        DegreeDifference = degreeDifference;
    }
}



