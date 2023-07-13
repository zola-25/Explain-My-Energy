using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast
{
    [FeatureState]
    public record HistoricalForecastState
    {
        public ImmutableList<DailyCostedReading> GasForecastDailyCosts { get; init; }
        public ImmutableList<DailyCostedReading> ElectricityForecastDailyCosts { get; init; }
        public DateTime ElectricityLastUpdate { get; init; }
        public DateTime GasLastUpdate { get; init; }

        public HistoricalForecastState()
        {
            GasForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
            ElectricityForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
            ElectricityLastUpdate = DateTime.MinValue;
            GasLastUpdate = DateTime.MinValue;
        }
    }

    public class StoreHistoricalForecastAction
    {
        public MeterType MeterType { get; }
        public ImmutableList<DailyCostedReading> ForecastDailyCosts { get; }

        public StoreHistoricalForecastAction(MeterType meterType, ImmutableList<DailyCostedReading> forecastDailyCosts)
        {
            MeterType = meterType;
            ForecastDailyCosts = forecastDailyCosts;
        }

        [ReducerMethod]
        public static HistoricalForecastState StoreHistoricalForecastReducer(HistoricalForecastState state, StoreHistoricalForecastAction action)
        {
            return action.MeterType switch
            {
                MeterType.Electricity => state with
                {
                    ElectricityForecastDailyCosts = action.ForecastDailyCosts,
                    ElectricityLastUpdate = DateTime.UtcNow
                },
                MeterType.Gas => state with
                {
                    GasForecastDailyCosts = action.ForecastDailyCosts,
                    GasLastUpdate = DateTime.UtcNow
                },
                _ => throw new ArgumentOutOfRangeException(nameof(action.MeterType)),
            };
        }

    }

    public class EnsureElectricityHistoricalForecastAction
    {
        bool ForceRefresh { get; }

        TaskCompletionSource<(bool result, string message)> TaskCompletionSource {get;}

        public EnsureElectricityHistoricalForecastAction(bool forceRefresh = false,
                                                         TaskCompletionSource<(bool result, string message)> taskCompletionSource = null)
        {
            ForceRefresh = forceRefresh;
            TaskCompletionSource = taskCompletionSource;
        }

        private class Effect : Effect<EnsureElectricityHistoricalForecastAction>
        {
            IState<HistoricalForecastState> _state;
            IState<ElectricityReadingsState> _electricityReadingsState;
            IState<MeterSetupState> _meterSetupState;
            ICostCalculator _costCalculator;
            ICostedReadingsToDailyAggregator _costedReadingsToDailyAggregator;

            public Effect(IState<HistoricalForecastState> state,
                          IState<ElectricityReadingsState> electricityReadingsState,
                          IState<MeterSetupState> meterSetupState,
                          ICostCalculator costCalculator,
                          ICostedReadingsToDailyAggregator costedReadingsToDailyAggregator)
            {
                _state = state;
                _electricityReadingsState = electricityReadingsState;
                _meterSetupState = meterSetupState;
                _costCalculator = costCalculator;
                _costedReadingsToDailyAggregator = costedReadingsToDailyAggregator;
            }

            public override Task HandleAsync(EnsureElectricityHistoricalForecastAction action, IDispatcher dispatcher)
            {
                if (_meterSetupState.Value.ElectricityMeter == null
                    || !_meterSetupState.Value.ElectricityMeter.SetupValid)
                {
                    action.TaskCompletionSource?.SetResult((false, "Electricity meter not setup"));
                    return Task.CompletedTask;
                }

                if (_meterSetupState.Value.ElectricityMeter.TariffDetails.eIsNullOrEmpty())
                {
                    action.TaskCompletionSource?.SetResult((false, "Electricity meter tariffs not setup"));
                    return Task.CompletedTask;
                }

                if(_electricityReadingsState.Value.BasicReadings.eIsNullOrEmpty())
                {
                    action.TaskCompletionSource?.SetResult((false, "No historical electricity readings"));
                    return Task.CompletedTask;
                }

                if(!action.ForceRefresh 
                    && _state.Value.ElectricityForecastDailyCosts.eIsNotNullOrEmpty() 
                    && _state.Value.ElectricityForecastDailyCosts.Count > 180
                    && _state.Value.ElectricityLastUpdate < DateTime.UtcNow.Date.AddDays(-7))
                {
                    action.TaskCompletionSource?.SetResult((true, "Forecast already exists"));
                    return Task.CompletedTask;
                }

                var basicReadings = _electricityReadingsState.Value.BasicReadings;
                var meterTariffs = _meterSetupState.Value.ElectricityMeter.TariffDetails;
                var costedReadings = GetCostedReadings(basicReadings, meterTariffs);

                var dailyCostedReadings = _costedReadingsToDailyAggregator.Aggregate(costedReadings).ToImmutableList();

                dispatcher.Dispatch(new StoreHistoricalForecastAction(MeterType.Electricity, dailyCostedReadings));

                action.TaskCompletionSource?.SetResult((true, "Electricty Historical Forecast created"));
                return Task.CompletedTask;
            }

            private List<CostedReading> GetCostedReadings(
                ImmutableList<BasicReading> historicalReadings,
                ImmutableList<TariffDetailState> meterTariffs)
            {
                var averageKWhByMonth = historicalReadings
                                    .GroupBy(c => c.UtcTime.Month)
                                    .Where(c => c.Count() >= 48 * 10)
                                    .ToDictionary(c => c.Key, c => c.Average(d => d.KWh));

                var startDate = DateTime.UtcNow.Date.AddMonths(-2);
                var forecastBasicReadings = Enumerable.Range(0, 48 * (180 + 60)).Select(i =>
                {
                    var readingDate = startDate.AddTicks(TimeSpan.TicksPerMinute * 30 * i);

                    // find the nearest month in the averageKWhByMonth dictionary choosing the earlier month if it's not found in the keys
                    // as a for loop would do
                    var monthToLookup = readingDate.Month;
                    int numberOfMonthsChecked = 0;
                    while (numberOfMonthsChecked <= 12)
                    {
                        if (averageKWhByMonth.ContainsKey(monthToLookup))
                        {
                            return new BasicReading
                            {
                                KWh = averageKWhByMonth[monthToLookup],
                                UtcTime = readingDate,
                                Forecast = true
                            };
                        }

                        monthToLookup--;
                        if (monthToLookup < 1)
                        {
                            monthToLookup = 12;
                        }
                        numberOfMonthsChecked++;
                    }

                    throw new Exception("No historical electricity data");
                }).ToList();

                var costedReadings = _costCalculator
                    .GetCostReadings(forecastBasicReadings, meterTariffs);
                return costedReadings;
            }
        }
    }
}
