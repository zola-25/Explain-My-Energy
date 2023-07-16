using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions
{
    public class EnsureElectricityHistoricalForecastAction
    {
        private bool ForceRefresh { get; }

        private TaskCompletionSource<(bool result, string message)> TaskCompletionSource { get; }

        public EnsureElectricityHistoricalForecastAction(bool forceRefresh = false,
                                                         TaskCompletionSource<(bool result, string message)> taskCompletionSource = null)
        {
            ForceRefresh = forceRefresh;
            TaskCompletionSource = taskCompletionSource;
        }

        private class Effect : Effect<EnsureElectricityHistoricalForecastAction>
        {
            private readonly IState<HistoricalForecastState> _state;
            private readonly IState<ElectricityReadingsState> _electricityReadingsState;
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IForecastReadingsMovingAverage _forecastReadingsMovingAverage;
            private readonly IHistoricalForecastValidation _historicalForecastValidation;

            public Effect(IState<HistoricalForecastState> state,
                          IState<ElectricityReadingsState> electricityReadingsState,
                          IState<MeterSetupState> meterSetupState,
                          IForecastReadingsMovingAverage forecastReadingsMovingAverage,
                          IHistoricalForecastValidation historicalForecastValidation)
            {
                _state = state;
                _electricityReadingsState = electricityReadingsState;
                _meterSetupState = meterSetupState;
                _forecastReadingsMovingAverage = forecastReadingsMovingAverage;
                _historicalForecastValidation = historicalForecastValidation;
            }

            public override Task HandleAsync(EnsureElectricityHistoricalForecastAction action, IDispatcher dispatcher)
            {
                bool validToContinue = _historicalForecastValidation.Validate(_meterSetupState.Value.ElectricityMeter,
                                                action.ForceRefresh,
                                                _state.Value.ElectricityLastUpdate,
                                                _electricityReadingsState.Value.BasicReadings,
                                                _state.Value.ElectricityForecastDailyCosts,
                                                action.TaskCompletionSource);

                if (!validToContinue)
                {
                    return Task.CompletedTask;
                }

                var historicalBasicReadings = _electricityReadingsState.Value.BasicReadings;
                var meterTariffs = _meterSetupState.Value.ElectricityMeter.TariffDetails;

                var forecastDailyCostedReadings = _forecastReadingsMovingAverage.GetDailyCostedReadings(historicalBasicReadings, meterTariffs);

                dispatcher.Dispatch(new StoreHistoricalForecastAction(MeterType.Electricity, forecastDailyCostedReadings));

                action.TaskCompletionSource?.SetResult((true, "Electricty Historical Forecast created"));
                return Task.CompletedTask;
            }


        }
    }
}
