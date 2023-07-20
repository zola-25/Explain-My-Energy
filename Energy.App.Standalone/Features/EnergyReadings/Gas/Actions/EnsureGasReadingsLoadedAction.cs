using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.Setup.Household;
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
            private readonly IState<HouseholdState> _householdState;
            private readonly IEnergyImportValidation _energyImportValidation;
            private readonly ICostCalculator _costCalculator;
            private readonly IEnergyReadingService _energyReadingService;
            private readonly ILogger<EnsureGasReadingsLoadedEffect> _logger;

            public EnsureGasReadingsLoadedEffect(IState<MeterSetupState> meterSetupState,
                                                 IState<GasReadingsState> gasReadingsState,
                                                 IEnergyImportValidation energyImportValidation,
                                                 ICostCalculator costCalculator,
                                                 IEnergyReadingService energyReadingService,
                                                 ILogger<EnsureGasReadingsLoadedEffect> logger,
                                                 IState<HouseholdState> householdState)
            {
                _meterSetupState = meterSetupState;
                _gasReadingsState = gasReadingsState;
                _energyImportValidation = energyImportValidation;
                _costCalculator = costCalculator;
                _energyReadingService = energyReadingService;
                _logger = logger;
                _householdState = householdState;
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
                    string message = String.Empty;
                    bool valid = false;
                    bool loadForecasts = false;
                    bool fullReloadForecasts = false;

                    switch (validationResult.UpdateType)
                    {
                        case UpdateType.NotValid:
                            message = validationResult.Message;
                            break;
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
                                
                                fullReloadForecasts = true;
                                message = $"{meterType} Readings: Updated {newBasicReadings.Count} readings";

                            }
                            else
                            {
                                message = $"{meterType} Readings: No new readings available";
                                loadForecasts = true;
    
                            }
                            valid = true;
                            break;


                        case UpdateType.FullReload:

                            var reloadedBasicReadings = (await _energyReadingService.ImportFromMoveInOrPreviousYear(meterType)).ToImmutableList();
                            var reloadedCostedReadings = CalculateCostedReadings(reloadedBasicReadings, meterSetup.TariffDetails);

                            dispatcher.Dispatch(new GasStoreReloadedReadingsAndCostsAction(reloadedBasicReadings, reloadedCostedReadings));

                            message = $"{meterType} Readings: Loaded {reloadedBasicReadings.Count} readings";
                            
                            fullReloadForecasts = true;
                            valid = true;
                            break;

                        case UpdateType.JustCosts:

                            var costedReadings = CalculateCostedReadings(existingBasicReadings, meterSetup.TariffDetails);
                            dispatcher.Dispatch(new GasStoreReloadedCostsOnlyAction(costedReadings));

                            message = $"{meterType} Readings: Updated {costedReadings.Count} cost readings only";
                            valid = true;
                            loadForecasts = true;
                            break;
                        case UpdateType.NoUpdateNeeded:

                            message = $"{meterType} Readings: Data already up to date";

                            valid = true;
                            loadForecasts = true;

                            break;

                        default:
                            throw new NotImplementedException(nameof(validationResult.UpdateType));
                    }


                    if (valid && (loadForecasts || fullReloadForecasts))
                    {
                        var forecastCompletion = new TaskCompletionSource<(bool, string)>();

                        dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(
                            forceRefresh: fullReloadForecasts, forecastCompletion));
                        
                        var heatingForecastCompletion = new TaskCompletionSource<(bool, string)>();
                        if (_householdState.Value.PrimaryHeatSource == meterType) {
                            dispatcher.Dispatch(new EnsureHeatingSetupAction(
                                forceReloadHeatingForecast: loadForecasts,
                                forceReloadCoefficients: fullReloadForecasts,
                                heatingForecastCompletion));
                        } else
                        {
                            heatingForecastCompletion.SetResult((true, "Not required"));
                        }

                        await Task.WhenAll(forecastCompletion.Task, heatingForecastCompletion.Task);
                    }

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(valid, message));
                    action.TaskCompletion?.SetResult((valid, message));

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
