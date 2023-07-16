using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class EnsureElectricityReadingsLoadedAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<int> TaskCompletion { get; }

        public EnsureElectricityReadingsLoadedAction(bool forceReload, TaskCompletionSource<int> taskCompletion = null)
        {
            ForceReload = forceReload;
            TaskCompletion = taskCompletion;
        }

        [ReducerMethod(typeof(EnsureElectricityReadingsLoadedAction))]
        public static ElectricityReadingsState StartElectricityReadingsLoadingReducer(ElectricityReadingsState state)
        {
            return state with { Loading = true };
        }

        [ReducerMethod]
        public static ElectricityReadingsState NotifyFinishedReducer(ElectricityReadingsState state, NotifyElectricityLoadingFinished action)
        {
            return state with
            {
                Loading = false,
                CalculationError = action.Error,
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState StoreReloadedReadingsAndCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedReadingsAndCostsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings,
                CostedReadings = action.CostedReadings,
                LastUpdated = DateTime.UtcNow,
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState StoreReloadedCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedCostsOnlyAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                LastUpdated = DateTime.UtcNow,
            };
        }


        private class Effect : Effect<EnsureElectricityReadingsLoadedAction>
        {
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IState<ElectricityReadingsState> _electricityReadingsState;

            private readonly IEnergyUpdateMethodService _energyUpdateMethodService;

            public Effect(IState<MeterSetupState> meterSetupState,
                          IState<ElectricityReadingsState> electricityReadingsState,
                          IEnergyUpdateMethodService energyUpdateMethodService)
            {
                _meterSetupState = meterSetupState;
                _electricityReadingsState = electricityReadingsState;
                _energyUpdateMethodService = energyUpdateMethodService;
            }

            public override async Task HandleAsync(EnsureElectricityReadingsLoadedAction action, IDispatcher dispatcher)
            {
                var meterType = MeterType.Electricity;
                var existingBasicReadings = _electricityReadingsState.Value.BasicReadings;
                var existingCostedReadings = _electricityReadingsState.Value.CostedReadings;

                var meterSetup = _meterSetupState.Value[meterType];
                bool updated = false;
                int basicReadingsUpdated = 0;

                try
                {
                    var updateAction = await _energyUpdateMethodService.GetUpdateMethod(meterSetup, action.ForceReload, existingBasicReadings, existingCostedReadings);

                    switch (updateAction.UpdateType)
                    {
                        case UpdateType.NotValid:
                            dispatcher.Dispatch(new NotifyElectricityLoadingFinished(false, "Meter not setup"));
                            action.TaskCompletion?.SetResult(0);
                            return;
                        case UpdateType.FullUpdate:
                            updated = true;
                            basicReadingsUpdated = updateAction.BasicReadingsUpdateCount;
                            dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAndCostsAction(updateAction.BasicReadings, updateAction.CostedReadings));
                            dispatcher.Dispatch(new EnsureElectricityHistoricalForecastAction(forceRefresh: true));
                            break;
                        case UpdateType.JustCosts:
                            updated = true;
                            dispatcher.Dispatch(new ElectricityStoreReloadedCostsOnlyAction(updateAction.CostedReadings));
                            break;
                        case UpdateType.NoUpdateNeeded:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                    dispatcher.Dispatch(new NotifyElectricityLoadingFinished(updated));

                    action.TaskCompletion?.SetResult(basicReadingsUpdated);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    action.TaskCompletion?.SetResult(0);

                    dispatcher.Dispatch(new NotifyElectricityLoadingFinished(false, e.Message));
                }
            }


        }

    }
}
