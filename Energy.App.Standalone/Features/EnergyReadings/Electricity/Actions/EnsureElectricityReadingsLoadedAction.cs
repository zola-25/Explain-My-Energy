using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class EnsureElectricityReadingsLoadedAction
{
    public bool ForceReload { get; }

    public TaskCompletionSource<(bool success, string message)> TaskCompletion { get; }

    public EnsureElectricityReadingsLoadedAction(bool forceReload, TaskCompletionSource<(bool success, string message)> taskCompletion = null)
    {
        ForceReload = forceReload;
        TaskCompletion = taskCompletion;
    }

    [ReducerMethod(typeof(EnsureElectricityReadingsLoadedAction))]
    public static ElectricityReadingsState StartElectricityReadingsLoadingReducer(ElectricityReadingsState state)
    {
        return state with { Loading = true };
    }

    [ReducerMethod(typeof(NotifyElectricityLoadingFinished))]
    public static ElectricityReadingsState NotifyFinishedReducer(ElectricityReadingsState state)
    {
        return state with
        {
            Loading = false,
        };
    }

    [ReducerMethod(typeof(ElectricityUpdateLastReadingsCheckAction))]
    public static ElectricityReadingsState UpdateLastReadingsCheckReducer(ElectricityReadingsState state)
    {
        return state with
        {
            LastCheckedForNewReadings = DateTime.UtcNow,
        };
    }

    [ReducerMethod]
    public static ElectricityReadingsState StoreReloadedReadingsAndCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedReadingsAndCostsAction action)
    {
        return state with
        {
            BasicReadings = action.BasicReadings,
            CostedReadings = action.CostedReadings,
            LastCheckedForNewReadings = DateTime.UtcNow,
        };
    }

    [ReducerMethod]
    public static ElectricityReadingsState StoreReloadedCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedCostsOnlyAction action)
    {
        return state with
        {
            CostedReadings = action.CostedReadings,
        };
    }


    private class EnsureElectricityReadingsLoadedEffect : Effect<EnsureElectricityReadingsLoadedAction>
    {
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<HouseholdState> _householdState;
        private readonly IEnergyImportValidation _energyImportValidation;
        private readonly ICostCalculator _costCalculator;
        private readonly IEnergyReadingService _energyReadingService;
        private readonly ILogger<EnsureElectricityReadingsLoadedEffect> _logger;

        public EnsureElectricityReadingsLoadedEffect(IState<MeterSetupState> meterSetupState,
                      IState<ElectricityReadingsState> electricityReadingsState,
                      IEnergyImportValidation energyImportValidation,
                      ICostCalculator costCalculator,
                      IEnergyReadingService energyReadingService,
                      ILogger<EnsureElectricityReadingsLoadedEffect> logger,
                      IState<HouseholdState> householdState)
        {
            _meterSetupState = meterSetupState;
            _electricityReadingsState = electricityReadingsState;
            _energyImportValidation = energyImportValidation;
            _costCalculator = costCalculator;
            _energyReadingService = energyReadingService;
            _logger = logger;
            _householdState = householdState;
        }

        public override async Task HandleAsync(EnsureElectricityReadingsLoadedAction action, IDispatcher dispatcher)
        {
            var meterType = MeterType.Electricity;
            var existingBasicReadings = _electricityReadingsState.Value.BasicReadings;
            var existingCostedReadings = _electricityReadingsState.Value.CostedReadings;
            var lastReadingsCheck = _electricityReadingsState.Value.LastCheckedForNewReadings;

            var meterSetup = _meterSetupState.Value[meterType];

            try
            {
                var validationResult = _energyImportValidation.Validate(meterSetup, action.ForceReload, lastReadingsCheck, existingBasicReadings, existingCostedReadings);
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

                        dispatcher.Dispatch(new ElectricityUpdateLastReadingsCheckAction());

                        bool anythingNew = newBasicReadings.Any() && newBasicReadings.Last().UtcTime > lastBasicReading;

                        if (anythingNew)
                        {
                            var basicReadingsToUpdate = existingBasicReadings.ToList();

                            // just in case there is an overlap
                            _ = basicReadingsToUpdate.RemoveAll(x => x.UtcTime >= newBasicReadings.First().UtcTime);
                            basicReadingsToUpdate.AddRange(newBasicReadings);

                            var newCostedReadings = CalculateCostedReadings(basicReadingsToUpdate, meterSetup.TariffDetails);
                            dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAndCostsAction(basicReadingsToUpdate.ToImmutableList(), newCostedReadings));

                            fullReloadForecasts = true;
                            message = $"{meterType} Readings: Updated {newBasicReadings.Count} readings";

                        }
                        else if (validationResult.IncludeCosts)
                        {
                            var missingCostedReadings = CalculateCostedReadings(existingBasicReadings, meterSetup.TariffDetails);
                            dispatcher.Dispatch(new ElectricityStoreReloadedCostsOnlyAction(missingCostedReadings));

                            message = $"{meterType} Readings: Updated {missingCostedReadings.Count} cost readings only";
                            loadForecasts = true;
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

                        dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAndCostsAction(reloadedBasicReadings, reloadedCostedReadings));

                        message = $"{meterType} Readings: Loaded {reloadedBasicReadings.Count} readings";

                        fullReloadForecasts = true;
                        valid = true;
                        break;

                    case UpdateType.JustCosts:

                        var costedReadings = CalculateCostedReadings(existingBasicReadings, meterSetup.TariffDetails);
                        dispatcher.Dispatch(new ElectricityStoreReloadedCostsOnlyAction(costedReadings));

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
                    var historicalForecastCompletion = new TaskCompletionSource<(bool, string)>();

                    dispatcher.Dispatch(new EnsureElectricityHistoricalForecastAction(
                        forceRefresh: fullReloadForecasts, historicalForecastCompletion));

                    var heatingForecastCompletion = new TaskCompletionSource<(bool, string)>();
                    if (_householdState.Value.PrimaryHeatSource != MeterType.Electricity)
                    {
                        heatingForecastCompletion.SetResult((true, String.Empty));
                    }
                    else
                    {
                        dispatcher.Dispatch(new EnsureHeatingSetupAction(
                            forceReloadHeatingForecast: loadForecasts,
                            forceReloadCoefficients: fullReloadForecasts,
                            heatingForecastCompletion));
                    }
                    (bool, string)[] forecastResults = await Task.WhenAll(historicalForecastCompletion.Task, heatingForecastCompletion.Task);

                    valid = valid && forecastResults.All(c => c.Item1);
                    message = String.Join(Environment.NewLine, message.eYield().Concat(forecastResults.Select(c => c.Item2)));
                }

                dispatcher.Dispatch(new NotifyElectricityLoadingFinished(valid, message));
                action.TaskCompletion?.SetResult((valid, message));

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading {MeterType} readings", meterType);

                string errorMessage = $"{meterType} Readings: Error loading readings";
                action.TaskCompletion?.SetResult((false, errorMessage));

                dispatcher.Dispatch(new NotifyElectricityLoadingFinished(false, errorMessage));
            }
        }

        private ImmutableList<CostedReading> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings, ImmutableList<TariffDetailState> tariffDetails)
        {
            var costedReadings = _costCalculator
                .GetCostReadings(basicReadings, tariffDetails).ToImmutableList();
            return costedReadings;
        }
    }
}
