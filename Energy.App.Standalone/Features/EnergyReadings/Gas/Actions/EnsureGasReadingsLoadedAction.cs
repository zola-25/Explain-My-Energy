using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class EnsureGasReadingsLoadedAction
    {
        public bool ForceReload { get; }
        public TaskCompletionSource<int> TaskCompletion { get; }

        public EnsureGasReadingsLoadedAction(bool forceReload, TaskCompletionSource<int> taskCompletion = null)
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
                LastUpdated = DateTime.UtcNow,
            };
        }

        [ReducerMethod]
        public static GasReadingsState StoreReloadedCostsReducer(GasReadingsState state, GasStoreReloadedCostsOnlyAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                LastUpdated = DateTime.UtcNow,
            };
        }


        private class Effect : Effect<EnsureGasReadingsLoadedAction>
        {
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IState<GasReadingsState> _gasReadingsState;

            private readonly IEnergyUpdateMethodService _energyUpdateMethodService;

            public Effect(IState<MeterSetupState> meterSetupState,
                          IState<GasReadingsState> gasReadingsState,
                          IEnergyUpdateMethodService energyUpdateMethodService)
            {
                _meterSetupState = meterSetupState;
                _gasReadingsState = gasReadingsState;
                _energyUpdateMethodService = energyUpdateMethodService;
            }

            public override async Task HandleAsync(EnsureGasReadingsLoadedAction action, IDispatcher dispatcher)
            {
                var meterType = MeterType.Gas;
                var existingBasicReadings = _gasReadingsState.Value.BasicReadings;
                var existingCostedReadings = _gasReadingsState.Value.CostedReadings;

                var meterSetup = _meterSetupState.Value[meterType];
                bool updated = false;
                int basicReadingsUpdated = 0;

                try
                {
                    var updateAction = await _energyUpdateMethodService.GetUpdateMethod(meterSetup, action.ForceReload, existingBasicReadings, existingCostedReadings);

                    switch (updateAction.UpdateType)
                    {
                        case UpdateType.NotValid:
                            dispatcher.Dispatch(new NotifyGasLoadingFinished(false, "Meter not setup"));
                            action.TaskCompletion?.SetResult(0);
                            return;
                        case UpdateType.FullUpdate:
                            updated = true;
                            basicReadingsUpdated = updateAction.BasicReadingsUpdateCount;
                            dispatcher.Dispatch(new GasStoreReloadedReadingsAndCostsAction(updateAction.BasicReadings, updateAction.CostedReadings));
                            dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(forceRefresh: true));
                            
                            break;
                        case UpdateType.JustCosts:
                            updated = true;
                            dispatcher.Dispatch(new GasStoreReloadedCostsOnlyAction(updateAction.CostedReadings));
                            break;
                        case UpdateType.NoUpdateNeeded:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(updated));

                    action.TaskCompletion?.SetResult(basicReadingsUpdated);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    action.TaskCompletion?.SetResult(0);

                    dispatcher.Dispatch(new NotifyGasLoadingFinished(false, e.Message));
                }
            }


        }

    }
}
