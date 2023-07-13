using System.Collections.Immutable;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class EnsureGasReadingsLoadedAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<int> TaskCompletion { get; }

        public EnsureGasReadingsLoadedAction(bool forceReload, TaskCompletionSource<int> taskCompletion)
        {
            ForceReload = forceReload;
            TaskCompletion = taskCompletion;
        }

        [ReducerMethod(typeof(EnsureGasReadingsLoadedAction))]
        public static GasReadingsState StartGasReadingsLoadingReducer(GasReadingsState state)
        {
            return state with { Loading = true };
        }

        [ReducerMethod]
        public static GasReadingsState NotifyFinishedReducer(GasReadingsState state, NotifyGasLoadingFinished action)
        {
            if (action.Updated && String.IsNullOrEmpty(action.Error))
            {
                return state with
                {
                    Loading = false,
                    LastUpdated = DateTime.UtcNow,
                    CalculationError = String.Empty
                };
            }

            return state with
            {
                Loading = false,
                CalculationError = action.Error,
            };
        }

        [ReducerMethod]
        public static GasReadingsState StoreReloadedReadingsAndCostsReducer(GasReadingsState state, GasStoreReloadedReadingsAndCostsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings,
                CostedReadings = action.CostedReadings,
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


        private class Effect : Effect<EnsureGasReadingsLoadedAction>
        {
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IState<GasReadingsState> _gasReadingsState;
            private readonly ICostCalculator _costCalculator;
            private readonly IEnergyReadingWorkerService _energyReadingWorkerService;

            public Effect(IState<MeterSetupState> meterSetupState,
                          IState<GasReadingsState> gasReadingsState,
                          ICostCalculator costCalculator,
                          IEnergyReadingWorkerService energyReadingWorkerService)
            {
                _meterSetupState = meterSetupState;
                _gasReadingsState = gasReadingsState;
                _costCalculator = costCalculator;
                _energyReadingWorkerService = energyReadingWorkerService;
            }



            // Loading on startup and stop on NotifyGasStoreReady and move reducers in here

            public override async Task HandleAsync(EnsureGasReadingsLoadedAction action, IDispatcher dispatcher)
            {
                bool updated = false;
                int basicReadingsUpdated = 0;
                if (!_meterSetupState.Value.GasMeter.SetupValid)
                {
                    dispatcher.Dispatch(new NotifyGasLoadingFinished(updated));
                    action.TaskCompletion.SetResult(0);
                    return;
                }

                try
                {

                    var existingBasicReadings = _gasReadingsState.Value.BasicReadings;
                    if (existingBasicReadings.eIsNullOrEmpty() || action.ForceReload)
                    {
                        var basicReadings = await _energyReadingWorkerService.ImportFromMoveInOrPreviousYear(MeterType.Gas);
                        var costedReadings = CalculateCostedReadings(basicReadings);
                        basicReadingsUpdated = basicReadings.Count;
                        dispatcher.Dispatch
                        (
                            new GasStoreReloadedReadingsAndCostsAction(basicReadings.ToImmutableList(), costedReadings)
                        );
                        updated = true;
                    }
                    else
                    {

                        var lastBasicReading = _gasReadingsState.Value.BasicReadings.Last().
                            UtcTime;

                        if (lastBasicReading < DateTime.UtcNow.Date.AddDays(-1))
                        {
                            var newBasicReadings = await _energyReadingWorkerService.ImportFromDate
                                (MeterType.Gas, lastBasicReading.Date);

                            var updatedBasicReadings = existingBasicReadings.ToList();
                            if (newBasicReadings.Any())
                            {
                                updatedBasicReadings.RemoveAll
                                (
                                    x => x.UtcTime >= newBasicReadings.First().
                                        UtcTime
                                ); // just in case there is an overlap
                                updatedBasicReadings.AddRange(newBasicReadings);
                            }

                            basicReadingsUpdated = newBasicReadings.Count;
                            var costedReadings = CalculateCostedReadings(updatedBasicReadings);
                            dispatcher.Dispatch
                            (
                                new GasStoreReloadedReadingsAndCostsAction
                                    (updatedBasicReadings.ToImmutableList(), costedReadings)
                            );
                            updated = true;

                        }
                        else
                        {
                            var costedReadings = _gasReadingsState.Value.CostedReadings;

                            if (costedReadings.eIsNullOrEmpty()
                                || costedReadings.Last().
                                    UtcTime <= lastBasicReading)
                            {
                                var newCostedReadings = CalculateCostedReadings(existingBasicReadings);
                                dispatcher.Dispatch(new GasStoreReloadedCostsOnlyAction(newCostedReadings));
                                updated = true;

                            }
                        }

                    }

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(updated));

                    action.TaskCompletion.SetResult(basicReadingsUpdated);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    action.TaskCompletion.SetResult(0);

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(false, e.Message));
                }
            }

            private ImmutableList<CostedReading> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings)
            {
                var costedReadings = _costCalculator
                    .GetCostReadings(basicReadings,
                        _meterSetupState.Value[MeterType.Gas].TariffDetails).ToImmutableList();
                return costedReadings;
            }
        }

    }
}
