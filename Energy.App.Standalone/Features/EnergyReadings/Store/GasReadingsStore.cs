using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.EnergyReadings.Store
{
    [PersistState]
    public record GasReadingsState
    {
        [property: JsonIgnore]
        public bool Reloading { get; init; }
        

        [property: JsonIgnore]
        public bool Updating { get; init; }

        [property: JsonIgnore]
        public bool CalculatingCosts { get; init; }


        public ImmutableList<BasicReading> BasicReadings { get; init; }



        [property: JsonIgnore]
        public ImmutableList<CostedReading> CostedReadings { get; init; }
        
        [property: JsonIgnore]
        public bool CalculationError { get; init; }
    }

    public class GasReadingsFeature : Feature<GasReadingsState>
    {
        public override string GetName()
        {
            return nameof(GasReadingsFeature);
        }

        protected override GasReadingsState GetInitialState()
        {
            return new GasReadingsState
            {
                Reloading = false,
                Updating = false,
                CalculatingCosts = false,
                CalculationError = false,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                CostedReadings = ImmutableList<CostedReading>.Empty
            };
        }
    }

    public class GasInitiateCostCalculationsAction
    {
    }

    public class GasInitiateDeleteReadingsAction
    { }

    public class GasExecuteDeleteReadingsAction
    { }

    public class NotifyGasReadingsDeletedAction
    { }

    public class GasReloadReadingsAction
    { }

    public class GasUpdateReadingsAction
    {
        public DateTime LastReading { get; }

        public GasUpdateReadingsAction(DateTime lastReading)
        {
            LastReading = lastReading;
        }
    }

    public class GasStoreReloadedReadingsAction
    {
        public List<BasicReading> BasicReadings { get; }

        public GasStoreReloadedReadingsAction(List<BasicReading> basicReadings)
        {
            BasicReadings = basicReadings;
        }
    }

    public class GasStoreUpdatedReadingsAction
    {
        public List<BasicReading> NewReadings { get; }

        public GasStoreUpdatedReadingsAction(List<BasicReading> newReadings)
        {
            NewReadings = newReadings;
        }
    }

    public class NotifyGasStoreReady { }

    public static class GasReadingsReducers
    {
        [ReducerMethod]
        public static GasReadingsState OnStoreReloadedReadingsReducer(GasReadingsState state, GasStoreReloadedReadingsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings.ToImmutableList(),
                Reloading = false
            };
        }


        [ReducerMethod(typeof(GasReloadReadingsAction))]
        public static GasReadingsState OnBeginReloadReadingsReducer(GasReadingsState state)
        {
            return state with
            {
                Reloading = true
            };
        }

        [ReducerMethod(typeof(GasUpdateReadingsAction))]
        public static GasReadingsState OnBeginUpdateReadingsReducer(GasReadingsState state)
        {
            return state with
            {
                Updating = true
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreUpdatedReadingsReducer(GasReadingsState state, GasStoreUpdatedReadingsAction action)
        {
            return state with
            {
                BasicReadings = state.BasicReadings.AddRange(action.NewReadings),
                Updating = false
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreInitiateDeleteReadingsReducer(GasReadingsState state, GasInitiateDeleteReadingsAction action)
        {
            return state with
            {
                Updating = true
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreExecuteDeleteReadingsReducer(GasReadingsState state, GasExecuteDeleteReadingsAction action)
        {
            return state with
            {
                BasicReadings = ImmutableList<BasicReading>.Empty,
                CalculatingCosts = false,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                Reloading = false,
                Updating = false
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreInitiateCostCalculationsReducer(GasReadingsState state, GasInitiateCostCalculationsAction action)
        {
            return state with
            {
                CalculatingCosts = true
            };
        }
        //GasStoreCostedReadingsAction
        [ReducerMethod]

        public static GasReadingsState OnStoreGasStoreCostedReadingsActionReducer(GasReadingsState state, GasStoreCostedReadingsAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                CalculatingCosts = false,
                CalculationError = false
            };
        }

        [ReducerMethod]

        public static GasReadingsState OnCostCalculationsFailedNotification(GasReadingsState state, NotifyGasCostsCalculationFailedAction action)
        {
            return state with
            {
                CalculatingCosts = false,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                CalculationError = true
            };
        }

        public class GasStoreCostCalculationsCompleteAction
        {
        }

        // Delete readings with subscribe
        // Tariffs
        // Weather Data status

        public class GasReadingsEffects
        {
            private readonly IEnergyReadingImporter _energyReadingImporter;
            private readonly ICostCalculator _energyCostCalculator;

            IState<GasReadingsState> _gasReadingsState;
            IState<GasTariffsState> _gasTariffsState;

            public GasReadingsEffects(IEnergyReadingImporter energyReadingImporter, IState<GasReadingsState> gasReadingsState, IState<GasTariffsState> gasTariffsState, ICostCalculator energyCostCalculator)
            {
                _energyReadingImporter = energyReadingImporter;
                _gasReadingsState = gasReadingsState;
                _gasTariffsState = gasTariffsState;
                _energyCostCalculator = energyCostCalculator;
            }

            [EffectMethod]
            public async Task ReloadGasReadings(GasReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
            {
                List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Gas);
                dispatcher.Dispatch(new GasStoreReloadedReadingsAction(basicReadings));
                dispatcher.Dispatch(new NotifyGasStoreReady());

            }

            [EffectMethod]
            public async Task UpdateGasReadings(GasUpdateReadingsAction updateReadingsAction, IDispatcher dispatcher)
            {
                List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromDate(MeterType.Gas, updateReadingsAction.LastReading);
                dispatcher.Dispatch(new GasStoreUpdatedReadingsAction(basicReadings));
                dispatcher.Dispatch(new NotifyGasStoreReady());

            }

            [EffectMethod]
            public Task DeleteGasReadings(GasInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
            {
                dispatcher.Dispatch(new GasExecuteDeleteReadingsAction());
                dispatcher.Dispatch(new NotifyGasReadingsDeletedAction());
                return Task.CompletedTask;

            }

            [EffectMethod]
            public Task CalculateCosts(GasInitiateCostCalculationsAction initiateCostCalculationsAction, IDispatcher dispatcher)
            {
                ImmutableList<CostedReading> costedReadings;
                try
                {
                    costedReadings = _energyCostCalculator.GetCostReadings(_gasReadingsState.Value.BasicReadings, _gasTariffsState.Value.TariffDetails);
                    dispatcher.Dispatch(new GasStoreCostedReadingsAction(costedReadings));
                    dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction());

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error calculating Gas reading costs:");
                    Console.WriteLine(e);

                    dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());
                    dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction(true));
                }

                return Task.CompletedTask;
            }
        }

        public class NotifyGasCostsCalculationFailedAction
        {
            public NotifyGasCostsCalculationFailedAction()
            {
            }
        }

        public class NotifyGasCostsCalculationCompletedAction
        {
            public bool CalculationError { get; }

            public NotifyGasCostsCalculationCompletedAction(bool calculationError = false)
            {
                CalculationError = calculationError;
            }

        }

        public class GasStoreCostedReadingsAction
        {
            public ImmutableList<CostedReading> CostedReadings { get; }

            public GasStoreCostedReadingsAction(ImmutableList<CostedReading> costedReadings)
            {
                CostedReadings = costedReadings;

            }
        }
    }
}
