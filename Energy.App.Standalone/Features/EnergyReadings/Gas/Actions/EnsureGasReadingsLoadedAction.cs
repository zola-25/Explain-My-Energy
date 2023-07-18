using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class EnsureGasReadingsLoadedAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<(bool success, string message)> TaskCompletion { get; }

        public EnsureGasReadingsLoadedAction(bool forceReload, TaskCompletionSource<(bool success, string message)> taskCompletion = null)
        {
            ForceReload = forceReload;
            TaskCompletion = taskCompletion;
        }

        [ReducerMethod(typeof(EnsureGasReadingsLoadedAction))]
        public static GasReadingsState StartGasReadingsLoadingReducer(GasReadingsState state)
        {
            return state with { Loading = true };
        }

        [ReducerMethod(typeof(NotifyGasLoadingFinished))]
        public static GasReadingsState NotifyFinishedReducer(GasReadingsState state)
        {
            return state with
            {
                Loading = false,
            };
        }


        [ReducerMethod]
        public static GasReadingsState StoreReloadedReadingsAndCostsReducer(GasReadingsState state, GasStoreReloadedReadingsAndCostsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings,
                CostedReadings = action.CostedReadings,
                LastUpdated = DateTime.UtcNow,
            };
        }

        [ReducerMethod]
        public static GasReadingsState StoreReloadedCostsReducer(GasReadingsState state, GasStoreReloadedCostsOnlyAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
            };
        }


        private class EnsureGasReadingsLoadedEffect : Effect<EnsureGasReadingsLoadedAction>
        {
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IState<GasReadingsState> _gasReadingsState;

            private readonly IEnergyImportValidation _energyImportValidation;
            private readonly ICostCalculator _costCalculator;
            private readonly IEnergyReadingService _energyReadingService;
            private readonly ILogger<EnsureGasReadingsLoadedEffect> _logger;

            public EnsureGasReadingsLoadedEffect(IState<MeterSetupState> meterSetupState,
                                                 IState<GasReadingsState> gasReadingsState,
                                                 IEnergyImportValidation energyImportValidation,
                                                 ICostCalculator costCalculator,
                                                 IEnergyReadingService energyReadingService,
                                                 ILogger<EnsureGasReadingsLoadedEffect> logger)
            {
                _meterSetupState = meterSetupState;
                _gasReadingsState = gasReadingsState;
                _energyImportValidation = energyImportValidation;
                _costCalculator = costCalculator;
                _energyReadingService = energyReadingService;
                _logger = logger;
            }

            public override async Task HandleAsync(EnsureGasReadingsLoadedAction action, IDispatcher dispatcher)
            {
                var meterType = MeterType.Gas;
                var existingBasicReadings = _gasReadingsState.Value.BasicReadings;
                var existingCostedReadings = _gasReadingsState.Value.CostedReadings;

                var meterSetup = _meterSetupState.Value[meterType];

                try
                {
                    var validationResult = _energyImportValidation.Validate(meterSetup, action.ForceReload, existingBasicReadings, existingCostedReadings);

                    switch (validationResult.UpdateType)
                    {
                        case UpdateType.NotValid:

                            dispatcher.Dispatch(new NotifyGasLoadingFinished(false, validationResult.Message));
                            action.TaskCompletion?.SetResult((false, validationResult.Message));
                            return;
                        case UpdateType.Update:
                            var lastBasicReading = existingBasicReadings.Last().UtcTime;
                            var newBasicReadings = await _energyReadingService.ImportFromDate(meterType, lastBasicReading.Date);

                            var basicReadingsToUpdate = existingBasicReadings.ToList();
                            if (newBasicReadings.Any())
                            {
                                // just in case there is an overlap
                                _ = basicReadingsToUpdate.RemoveAll(x => x.UtcTime >= newBasicReadings.First().UtcTime);
                                basicReadingsToUpdate.AddRange(newBasicReadings);

                                var newCostedReadings = CalculateCostedReadings(basicReadingsToUpdate, meterSetup.TariffDetails);
                                dispatcher.Dispatch(new GasStoreReloadedReadingsAndCostsAction(newBasicReadings.ToImmutableList(), newCostedReadings));
                                dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(forceRefresh: true));

                                var updateMessage = $"{meterType} Readings: Updated {newBasicReadings.Count} readings";
                                dispatcher.Dispatch(new NotifyGasLoadingFinished(true, updateMessage));
                                action.TaskCompletion?.SetResult((true, updateMessage));
                            }
                            else
                            {
                                var updateMessage = $"{meterType} Readings: No new readings available";
                                dispatcher.Dispatch(new NotifyGasLoadingFinished(true, updateMessage));
                                action.TaskCompletion?.SetResult((true, updateMessage));
                            }
                            return;


                        case UpdateType.FullReload:

                            var reloadedBasicReadings = (await _energyReadingService.ImportFromMoveInOrPreviousYear(meterType)).ToImmutableList();
                            var reloadedCostedReadings = CalculateCostedReadings(reloadedBasicReadings, meterSetup.TariffDetails);

                            dispatcher.Dispatch(new GasStoreReloadedReadingsAndCostsAction(reloadedBasicReadings, reloadedCostedReadings));
                            dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(forceRefresh: true));

                            var reloadMessage = $"{meterType} Readings: Loaded {reloadedBasicReadings.Count} readings";
                            dispatcher.Dispatch(new NotifyGasLoadingFinished(true, reloadMessage));
                            action.TaskCompletion?.SetResult((true, reloadMessage));
                            return;

                        case UpdateType.JustCosts:

                            var costedReadings = CalculateCostedReadings(existingBasicReadings, meterSetup.TariffDetails);
                            dispatcher.Dispatch(new GasStoreReloadedCostsOnlyAction(costedReadings));

                            var costMessage = $"{meterType} Readings: Updated {costedReadings.Count} cost readings only";
                            dispatcher.Dispatch(new NotifyGasLoadingFinished(true, costMessage));

                            action.TaskCompletion?.SetResult((true, costMessage));
                            return;
                        case UpdateType.NoUpdateNeeded:

                            var noUpdateMessage = $"{meterType} Readings: Data already up to date";

                            dispatcher.Dispatch(new NotifyGasLoadingFinished(true, noUpdateMessage));
                            action.TaskCompletion?.SetResult((true, noUpdateMessage));

                            return;

                        default:
                            throw new NotImplementedException(nameof(validationResult.UpdateType));

                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading {MeterType} readings", meterType);

                    var errorMessage = $"{meterType} Readings: Error loading readings";
                    action.TaskCompletion?.SetResult((false, errorMessage));

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(false, errorMessage));
                }
            }

            private ImmutableList<CostedReading> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings, ImmutableList<TariffDetailState> tariffDetails)
            {
                var costedReadings = _costCalculator
                    .GetCostReadings(basicReadings,
                        tariffDetails).ToImmutableList();
                return costedReadings;
            }


        }

    }
}
