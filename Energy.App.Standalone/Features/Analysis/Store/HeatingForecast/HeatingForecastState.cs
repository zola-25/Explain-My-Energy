using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using MathNet.Numerics;
using Fluxor.Persist.Storage;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using System.Text.Json.Serialization;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast
{
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
    }

    public class HeatingForecastFeature : Feature<HeatingForecastState>
    {
        public override string GetName()
        {
            return nameof(HeatingForecastFeature);
        }

        protected override HeatingForecastState GetInitialState()
        {

            return new HeatingForecastState()
            {

                ForecastsLastUpdate = DateTime.MinValue,
                SavedCoefficients = false,
                CoefficientsLastUpdate = DateTime.MinValue,
                C = 0,
                Gradient = 0,
                ForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty,
                ForecastWeatherReadings = ImmutableList<TemperaturePoint>.Empty
            };
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
                ForecastWeatherReadings = action.TemperatureIconPoints,
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
                                          IState<MeterSetupState> meterSetupState)
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
        }

        [EffectMethod]

        public async Task HandleInitiateUpdateLinearCoeffiecientsAction(InitiateCoefficientsAndLoadForecastAction action, IDispatcher dispatcher)
        {
            IEnumerable<BasicReading> basicReadings;
            var heatingMeter = _householdState.Value.PrimaryHeatSource;
            switch (heatingMeter)
            {
                case MeterType.Gas:
                    basicReadings = _gasReadingsState.Value.CostedReadings.Select(c => new BasicReading
                    {
                        Forecast = c.IsForecast,
                        KWh = c.KWh,
                        UtcTime = c.UtcTime
                    });
                    break;
                default:
                    basicReadings = _electricityReadingsState.Value.CostedReadings.Select(c => new BasicReading
                    {
                        Forecast = c.IsForecast,
                        KWh = c.KWh,
                        UtcTime = c.UtcTime
                    });
                    break;
            }

            var linearCoefficients = _forecastCoefficientsCreator
                .GetForecastCoefficients(basicReadings, _weatherState.Value.WeatherReadings);

            

            dispatcher.Dispatch(new StoreLinearCoefficientsStateAction(linearCoefficients.Gradient, linearCoefficients.C));
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
            var dailyAggregatedCostedReadings = costedReadingsForecast.GroupBy(c => c.UtcTime.Date).
                Select
                (
                    c => new DailyCostedReading()
                    {
                        UtcTime = c.Key,
                        ReadingTotalCostPence = c.Sum(d => d.CostPence).Round(2),
                        TariffAppliesFrom = c.First().
                            TarrifAppliesFrom,
                        TariffDailyStandingChargePence = c.First().
                            TariffDailyStandingCharge,
                        PencePerKWh = c.Average(d => d.TariffPencePerKWh).Round(2),
                        IsFixedCostPerHour = c.First().
                            IsFixedCostPerHour,
                        KWh = c.Sum(d => d.KWh).Round(2),
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
                        TemperatureCelsius = c.TemperatureAverage + degreeDifference,
                        TemperatureCelsiusUnmodified = c.TemperatureAverage,
                        Summary = c.Summary ?? string.Empty
                    }
                ).
                ToImmutableList();

            dispatcher.Dispatch(new StoreHeatingForecastStateAction(dailyAggregatedCostedReadings, temperatureIconPoints));
            dispatcher.Dispatch(new NotifyHeatingForecastReadyAction(degreeDifference));
        }


        [EffectMethod]
        public async Task HandleUpdateIfSignificate(UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction action, IDispatcher dispatcher)
        {
            if (_householdState.Value.PrimaryHeatSource != action.MeterType)
            {
                return;
            }
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
                dispatcher.Dispatch(new LoadHeatingForecastAction());
            } else
            {
                dispatcher.Dispatch(new NotifyHeatingForecastReadyAction());
            }
        }

    }















}
