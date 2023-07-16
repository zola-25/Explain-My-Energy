using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using MathNet.Numerics;
using Fluxor.Persist.Storage;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using System.Text.Json.Serialization;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast
{
    [FeatureState(Name = nameof(HeatingForecastState))]
    [PersistState]
    public record HeatingForecastState
    {

        [property: JsonIgnore]
        public ImmutableList<DailyCostedReading> ForecastDailyCosts { get; init; }

        [property: JsonIgnore]
        public ImmutableList<TemperaturePoint> ForecastWeatherReadings { get; init; }

        public DateTime CoefficientsLastUpdate { get; init; }
        public DateTime ForecastsLastUpdate { get; init; }

        public bool SavedCoefficients { get; init; }
        public decimal Gradient { get; init; }
        public decimal C { get; init; }

        public decimal PredictTemperatureX(decimal yConsumption)
        {
            return (yConsumption - C) / Gradient;
        }

        public decimal PredictConsumptionY(decimal xTemperature)
        {
            return Gradient * xTemperature + C;
        }


        public IEnumerable<(decimal temperatureX, decimal consumptionY)> Plot(decimal xMinTemp, decimal xMaxTemp)
        {
            var interval = (xMaxTemp - xMinTemp) / 100m;

            return Enumerable.Range(0, 100)
                .Select(c =>
                {
                    var x = xMinTemp + interval * c;
                    return (x, PredictConsumptionY(x));
                });
        }

        public HeatingForecastState()
        {
            ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
            ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty;
            CoefficientsLastUpdate = DateTime.MinValue;
            ForecastsLastUpdate = DateTime.MinValue;
            SavedCoefficients = false;
            Gradient = 0;
            C = 0;
        }
    }


    public static class HeatingForecastReducers
    {
        [ReducerMethod]
        public static HeatingForecastState UpdateLinearCoefficients(HeatingForecastState state, StoreLinearCoefficientsStateAction action)
        {
            return state with
            {
                SavedCoefficients = true,
                Gradient = action.Gradient,
                C = action.C,
                CoefficientsLastUpdate = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static HeatingForecastState ReduceLoadHeatingForecastAction(HeatingForecastState state,
            StoreHeatingForecastStateAction action)
        {
            return state with
            {
                ForecastDailyCosts = action.ForecastDailyReadings,
                ForecastWeatherReadings = action.TemperaturePoints,
                ForecastsLastUpdate = DateTime.UtcNow
            };
        }
    }

    // generate an effects class
    // 

    public class HeatingForecastEffects
    {
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IForecastGenerator _forecastGenerator;
        private readonly ICostCalculator _costCalculator;
        private readonly IState<HeatingForecastState> _heatingForecastState;
        private readonly IState<AnalysisOptionsState> _analysisOptionsState;
        private readonly IForecastCoefficientsCreator _forecastCoefficientsCreator;
        private readonly ICostedReadingsToDailyAggregator _costedReadingsToDailyAggregator;

        private readonly IState<MeterSetupState> _meterSetupState;

        public HeatingForecastEffects(IState<GasReadingsState> gasReadingsState,
                                          IState<ElectricityReadingsState> electricityReadingsState,
                                          IState<HouseholdState> householdState,
                                          IState<WeatherState> weatherState,
                                          IForecastGenerator forecastGenerator,
                                          ICostCalculator costCalculator,
                                          IForecastCoefficientsCreator forecastCoefficientsCreator,
                                          IState<HeatingForecastState> linearCoefficientsState,
                                          IState<AnalysisOptionsState> analysisOptionsState,
                                          IState<MeterSetupState> meterSetupState,
                                          ICostedReadingsToDailyAggregator costedReadingsToDailyAggregator)
        {
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
            _householdState = householdState;
            _weatherState = weatherState;
            _forecastGenerator = forecastGenerator;
            _costCalculator = costCalculator;
            _forecastCoefficientsCreator = forecastCoefficientsCreator;
            _heatingForecastState = linearCoefficientsState;
            _analysisOptionsState = analysisOptionsState;
            _meterSetupState = meterSetupState;
            _costedReadingsToDailyAggregator = costedReadingsToDailyAggregator;
        }

        [EffectMethod(typeof(InitiateCoefficientsAndLoadForecastAction))]

        public async Task HandleInitiateUpdateLinearCoeffiecientsAction(IDispatcher dispatcher)
        {
            var heatingMeter = _householdState.Value.PrimaryHeatSource;
            var basicReadings = heatingMeter switch
            {
                MeterType.Gas => _gasReadingsState.Value.CostedReadings.Select(c => new BasicReading
                {
                    Forecast = c.IsForecast,
                    KWh = c.KWh,
                    UtcTime = c.UtcTime
                }),
                MeterType.Electricity => _electricityReadingsState.Value.CostedReadings.Select(c => new BasicReading
                {
                    Forecast = c.IsForecast,
                    KWh = c.KWh,
                    UtcTime = c.UtcTime
                }),
                _ => throw new NotImplementedException()
            };


            var (C, Gradient) = _forecastCoefficientsCreator
                .GetForecastCoefficients(basicReadings, _weatherState.Value.WeatherReadings);

            dispatcher.Dispatch(new StoreLinearCoefficientsStateAction(Gradient, C));
            dispatcher.Dispatch(new LoadHeatingForecastAction(_analysisOptionsState?.Value[heatingMeter]?.DegreeDifference ?? 0));
        }

        [EffectMethod]
        public async Task HandleLoadHeatingForecastAction(LoadHeatingForecastAction action, IDispatcher dispatcher)
        {
            var heatingMeter = _householdState.Value.PrimaryHeatSource;
            decimal degreeDifference = action.DegreeDifference;


            var tariffs = _meterSetupState.Value[heatingMeter].TariffDetails;

            var recentWeatherReadings = _weatherState.Value.WeatherReadings.Where
                (
                    c => c.UtcTime >= DateTime.UtcNow.AddMonths(-2).
                        Date
                ).
                ToList();
            var basicReadingsForecast = _forecastGenerator.GetBasicReadingsForecast
                (degreeDifference, _heatingForecastState.Value, recentWeatherReadings);

            var costedReadingsForecast = _costCalculator.GetCostReadings(basicReadingsForecast, tariffs);
            var dailyAggregatedCostedReadings = 
                _costedReadingsToDailyAggregator
                    .Aggregate(costedReadingsForecast)
                    .ToImmutableList();

            var temperaturePoints = recentWeatherReadings.Select
                (
                    c => new TemperaturePoint()
                    {
                        UtcTime = c.UtcTime,
                        DateTicks = c.UtcTime.eToUnixTicksNoOffset(),
                        TemperatureCelsius = c.TemperatureAverage + degreeDifference,
                        TemperatureCelsiusUnmodified = c.TemperatureAverage,
                        Summary = c.Summary ?? string.Empty
                    }
                ).
                ToImmutableList();

            dispatcher.Dispatch(new StoreHeatingForecastStateAction(dailyAggregatedCostedReadings, temperaturePoints));
            dispatcher.Dispatch(new NotifyHeatingForecastReadyAction(degreeDifference));
        }


        [EffectMethod]
        public async Task HandleUpdateIfSignificate(UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction action, IDispatcher dispatcher)
        {
            var meterType = _householdState.Value.PrimaryHeatSource;
            if (!_meterSetupState.Value[meterType].SetupValid)
            {
                action.Completion?.SetResult(true);
                return;
            }

            decimal degreeDifference = _analysisOptionsState?.Value?[meterType]?.DegreeDifference ?? 0;
            
            dispatcher.Dispatch(new NotifyHeatingForecastUpdatingAction());

            if (action.SizeOfUpdate >= 7 * 48
                || !_heatingForecastState.Value.SavedCoefficients
                || _heatingForecastState.Value.CoefficientsLastUpdate < DateTime.Today.AddDays(-7))
            {
                dispatcher.Dispatch(new InitiateCoefficientsAndLoadForecastAction());
            }
            else if (_heatingForecastState.Value.ForecastWeatherReadings.eIsNullOrEmpty()
                || _heatingForecastState.Value.ForecastDailyCosts.eIsNullOrEmpty()
                || _heatingForecastState.Value.ForecastsLastUpdate < DateTime.Today.AddDays(-1))
            {
                dispatcher.Dispatch(new LoadHeatingForecastAction(degreeDifference));
            } else
            {
                dispatcher.Dispatch(new NotifyHeatingForecastReadyAction(degreeDifference));
            }
            action.Completion?.SetResult(true);
        }

    }















}
