using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;
using Fluxor;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;

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



        private class EnsureGasHistoricalForecastEffect : Effect<EnsureGasHistoricalForecastAction>
        {
            private readonly IState<HistoricalForecastState> _state;
            private readonly IState<GasReadingsState> _gasReadingsState;
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IForecastReadingsMovingAverage _forecastReadingsMovingAverage;
            private readonly IHistoricalForecastValidation _historicalForecastValidation;
            private readonly ILogger<EnsureGasHistoricalForecastEffect> _logger;

            public EnsureGasHistoricalForecastEffect(IState<HistoricalForecastState> state,
                          IState<GasReadingsState> gasReadingsState,
                          IState<MeterSetupState> meterSetupState,
                          IForecastReadingsMovingAverage forecastReadingsMovingAverage,
                          IHistoricalForecastValidation historicalForecastValidation,
                          ILogger<EnsureGasHistoricalForecastEffect> logger)
            {
                _state = state;
                _gasReadingsState = gasReadingsState;
                _meterSetupState = meterSetupState;
                _forecastReadingsMovingAverage = forecastReadingsMovingAverage;
                _historicalForecastValidation = historicalForecastValidation;
                _logger = logger;
            }

            public override Task HandleAsync(EnsureGasHistoricalForecastAction action, IDispatcher dispatcher)
            {
                var meterType = MeterType.Gas;
                try
                {
                    var validationResult = _historicalForecastValidation.Validate(
                                                    _meterSetupState.Value.GasMeter,
                                                    action.ForceRefresh,
                                                    _state.Value.GasLastUpdate,
                                                    _gasReadingsState.Value.BasicReadings,
                                                    _state.Value.GasForecastDailyCosts);

                    if (!validationResult.CanUpdate)
                    {
                        if (validationResult.IsWarning)
                        {
                            dispatcher.Dispatch(new NotifyGasForecastResult(false, validationResult.Message));
                            action.TaskCompletionSource?.SetResult((false, validationResult.Message));
                        }
                        else
                        {
                            dispatcher.Dispatch(new NotifyGasForecastResult(true, validationResult.Message));
                            action.TaskCompletionSource?.SetResult((true, validationResult.Message));

                        }
                        return Task.CompletedTask;
                    }

                    var historicalBasicReadings = _gasReadingsState.Value.BasicReadings;
                    var meterTariffs = _meterSetupState.Value.GasMeter.TariffDetails;

                    var forecastDailyCostedReadings = _forecastReadingsMovingAverage.GetDailyCostedReadings(historicalBasicReadings, meterTariffs);

                    dispatcher.Dispatch(new StoreHistoricalForecastAction(meterType, forecastDailyCostedReadings));

                    var resultMessage = @$"{meterType} Historical Forecast created 
                                            with {forecastDailyCostedReadings.Count} daily readings";

                    dispatcher.Dispatch(new NotifyGasForecastResult(true, resultMessage));

                    action.TaskCompletionSource?.SetResult((true, resultMessage));
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ensuring {MeterType} historical forecast", meterType);

                    var resultMessage = $"Error ensuring {meterType} historical forecast";

                    dispatcher.Dispatch(new NotifyGasForecastResult(false, resultMessage));
                    action.TaskCompletionSource?.SetResult((false, resultMessage));
                    return Task.CompletedTask;
                }
            }
        }
    }
}
