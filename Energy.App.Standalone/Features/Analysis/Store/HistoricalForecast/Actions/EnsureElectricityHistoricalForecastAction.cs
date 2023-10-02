using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;

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

    private class EnsureElectricityHistoricalForecastEffect : Effect<EnsureElectricityHistoricalForecastAction>
    {
        private readonly IState<HistoricalForecastState> _state;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IForecastReadingsMovingAverage _forecastReadingsMovingAverage;
        private readonly IHistoricalForecastValidation _historicalForecastValidation;
        private readonly ILogger<EnsureElectricityHistoricalForecastEffect> _logger;

        public EnsureElectricityHistoricalForecastEffect(IState<HistoricalForecastState> state,
                      IState<ElectricityReadingsState> electricityReadingsState,
                      IState<MeterSetupState> meterSetupState,
                      IForecastReadingsMovingAverage forecastReadingsMovingAverage,
                      IHistoricalForecastValidation historicalForecastValidation,
                      ILogger<EnsureElectricityHistoricalForecastEffect> logger)
        {
            _state = state;
            _electricityReadingsState = electricityReadingsState;
            _meterSetupState = meterSetupState;
            _forecastReadingsMovingAverage = forecastReadingsMovingAverage;
            _historicalForecastValidation = historicalForecastValidation;
            _logger = logger;
        }

        public override Task HandleAsync(EnsureElectricityHistoricalForecastAction action, IDispatcher dispatcher)
        {
            var meterType = MeterType.Electricity;
            try
            {
                var validationResult = _historicalForecastValidation.Validate(_meterSetupState.Value.ElectricityMeter,
                                                action.ForceRefresh,
                                                _state.Value.ElectricityLastUpdate,
                                                _electricityReadingsState.Value.BasicReadings,
                                                _state.Value.ElectricityForecastDailyCosts);

                if (!validationResult.CanUpdate)
                {
                    if (validationResult.IsWarning)
                    {
                        dispatcher.Dispatch(new NotifyElectricityForecastResult(false, validationResult.Message));
                        action.TaskCompletionSource?.SetResult((false, validationResult.Message));
                    }
                    else
                    {
                        dispatcher.Dispatch(new NotifyElectricityForecastResult(true, validationResult.Message));
                        action.TaskCompletionSource?.SetResult((true, validationResult.Message));

                    }
                    return Task.CompletedTask;
                }

                var historicalBasicReadings = _electricityReadingsState.Value.BasicReadings;
                var meterTariffs = _meterSetupState.Value.ElectricityMeter.TariffDetails;

                var forecastDailyCostedReadings = _forecastReadingsMovingAverage.GetDailyCostedReadings(historicalBasicReadings, meterTariffs);

                dispatcher.Dispatch(new StoreHistoricalForecastAction(meterType, forecastDailyCostedReadings));

                var resultMessage = $"{meterType} Historical Forecast created with {forecastDailyCostedReadings.Count} daily readings";
                dispatcher.Dispatch(new NotifyElectricityForecastResult(true, resultMessage));

                action.TaskCompletionSource?.SetResult((true, resultMessage));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring {MeterType} historical forecast", meterType);

                var resultMessage = $"Error ensuring {meterType} historical forecast";
                dispatcher.Dispatch(new NotifyElectricityForecastResult(false, resultMessage));
                action.TaskCompletionSource?.SetResult((false, resultMessage));
                return Task.CompletedTask;

            }
        }
    }
}
