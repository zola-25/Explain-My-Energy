using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;
using Fluxor;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions
{
    public class EnsureGasHistoricalForecastAction
    {
        private bool ForceRefresh { get; }

        private TaskCompletionSource<(bool result, string message)> TaskCompletionSource { get; }

        public EnsureGasHistoricalForecastAction(bool forceRefresh = false,
                                                 TaskCompletionSource<(bool result, string message)> taskCompletionSource = null)
        {
            ForceRefresh = forceRefresh;
            TaskCompletionSource = taskCompletionSource;
        }

        private class Effect : Effect<EnsureGasHistoricalForecastAction>
        {
            private readonly IState<HistoricalForecastState> _state;
            private readonly IState<GasReadingsState> _gasReadingsState;
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IForecastReadingsMovingAverage _forecastReadingsMovingAverage;
            private readonly IHistoricalForecastValidation _historicalForecastValidation;

            public Effect(IState<HistoricalForecastState> state,
                          IState<GasReadingsState> gasReadingsState,
                          IState<MeterSetupState> meterSetupState,
                          IForecastReadingsMovingAverage forecastReadingsMovingAverage,
                          IHistoricalForecastValidation historicalForecastValidation)
            {
                _state = state;
                _gasReadingsState = gasReadingsState;
                _meterSetupState = meterSetupState;
                _forecastReadingsMovingAverage = forecastReadingsMovingAverage;
                _historicalForecastValidation = historicalForecastValidation;
            }

            public override Task HandleAsync(EnsureGasHistoricalForecastAction action, IDispatcher dispatcher)
            {
                bool validToContinue = _historicalForecastValidation.Validate(_meterSetupState.Value.GasMeter,
                                                                      action.ForceRefresh,
                                                                      _state.Value.GasLastUpdate,
                                                                      _gasReadingsState.Value.BasicReadings,
                                                                      _state.Value.GasForecastDailyCosts,
                                                                      action.TaskCompletionSource);
                if (!validToContinue)
                {
                    return Task.CompletedTask;
                }
                var historicalBasicReadings = _gasReadingsState.Value.BasicReadings;
                var meterTariffs = _meterSetupState.Value.GasMeter.TariffDetails;

                var forecastDailyCostedReadings = _forecastReadingsMovingAverage.GetDailyCostedReadings(historicalBasicReadings, meterTariffs);

                dispatcher.Dispatch(new StoreHistoricalForecastAction(MeterType.Gas, forecastDailyCostedReadings));

                action.TaskCompletionSource?.SetResult((true, "Gas Historical Forecast created"));
                return Task.CompletedTask;
            }

        }
    }
}
