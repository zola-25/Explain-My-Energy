using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using static Energy.App.Standalone.Features.EnergyReadings.Store.GasReadingsReducers;

namespace Energy.App.Standalone.Features.EnergyReadings.Store
{
    [PersistState]
    public record ElectricityReadingsState
    {
        [property: JsonIgnore]
        public bool ReloadingReadings { get; init; }
        
        [property: JsonIgnore]
        public bool UpdatingReadings { get; init; }
        
        [property: JsonIgnore]
        public bool CalculatingCosts { get; init; }


        public ImmutableList<BasicReading> BasicReadings { get; init; }

        [property: JsonIgnore]
        public ImmutableList<CostedReading> CostedReadings { get; init; }

        [property: JsonIgnore]
        public bool CalculationError { get; init; }
    }

    public class ElectricityReadingsFeature : Feature<ElectricityReadingsState>
    {
        public override string GetName()
        {
            return nameof(ElectricityReadingsFeature);
        }

        protected override ElectricityReadingsState GetInitialState()
        {
            return new ElectricityReadingsState
            {
                ReloadingReadings = false,
                UpdatingReadings = false,
                CalculatingCosts = false,
                CalculationError = false,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                CostedReadings = ImmutableList<CostedReading>.Empty
            };
        }
    }

    public class ElectricityInitiateCostCalculationsAction
    {
    }


    public class ElectricityInitiateDeleteReadingsAction
    { }

    public class ElectricityExecuteDeleteReadingsAction
    { }

    public class NotifyElectricityReadingsDeletedAction
    { }

    public class ElectricityReloadReadingsAction
    { }

    public class ElectricityUpdateReadingsAction
    {
        public DateTime LastReading { get; }

        public ElectricityUpdateReadingsAction(DateTime lastReading)
        {
            LastReading = lastReading;
        }
    }

    public class ElectricityStoreReloadedReadingsAction
    {
        public List<BasicReading> BasicReadings { get; }

        public ElectricityStoreReloadedReadingsAction(List<BasicReading> basicReadings)
        {
            BasicReadings = basicReadings;
        }
    }

    public class ElectricityStoreUpdatedReadingsAction
    {
        public List<BasicReading> NewReadings { get; }

        public ElectricityStoreUpdatedReadingsAction(List<BasicReading> newReadings)
        {
            NewReadings = newReadings;
        }
    }

    public class NotifyElectricityStoreReady { }


    public static class ElectricityReadingsReducers
    {
        


        [ReducerMethod]
        public static ElectricityReadingsState OnStoreReloadedReadingsReducer(ElectricityReadingsState state, ElectricityStoreReloadedReadingsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings.ToImmutableList(),
                ReloadingReadings = false
            };
        }


        [ReducerMethod(typeof(ElectricityReloadReadingsAction))]
        public static ElectricityReadingsState OnBeginReloadReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingReadings = true
            };
        }

        [ReducerMethod(typeof(ElectricityUpdateReadingsAction))]
        public static ElectricityReadingsState OnBeginUpdateReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                UpdatingReadings = true
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreUpdatedReadingsReducer(ElectricityReadingsState state, ElectricityStoreUpdatedReadingsAction action)
        {
            return state with
            {
                BasicReadings = state.BasicReadings.AddRange(action.NewReadings),
                UpdatingReadings = false
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreInitiateDeleteReadingsReducer(ElectricityReadingsState state, ElectricityInitiateDeleteReadingsAction action)
        {
            return state with
            {
                UpdatingReadings = true
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreExecuteDeleteReadingsReducer(ElectricityReadingsState state, ElectricityExecuteDeleteReadingsAction action)
        {
            return state with
            {
                BasicReadings = ImmutableList<BasicReading>.Empty,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                UpdatingReadings = false
            };
        }


        [ReducerMethod]
        public static ElectricityReadingsState OnInitiateCostCalculationsReducer(ElectricityReadingsState state, ElectricityInitiateCostCalculationsAction action)
        {
            return state with
            {
                CalculatingCosts = true
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreExecuteCostCalculationsReducer(ElectricityReadingsState state, ElectricityStoreCostedReadingsAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                CalculatingCosts = false,
                CalculationError = false
            };
        }

        [ReducerMethod]

        public static ElectricityReadingsState OnCostCalculationsFailedNotification(ElectricityReadingsState state, NotifyElectricityCostsCalculationFailedAction action)
        {
            return state with
            {
                CalculatingCosts = false,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                CalculationError = true
            };
        }

    }

    public class ElectricityReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;
        private readonly ICostCalculator _energyCostCalculator;
        IState<ElectricityTariffsState> _electricityTariffsState;
        IState<ElectricityReadingsState> _electricityReadingsState;

        public ElectricityReadingsEffects(IEnergyReadingImporter energyReadingImporter, IState<ElectricityTariffsState> electricityTariffsState, ICostCalculator energyCostCalculator, IState<ElectricityReadingsState> electricityReadingsState)
        {
            _energyReadingImporter = energyReadingImporter;
            _electricityTariffsState = electricityTariffsState;
            _energyCostCalculator = energyCostCalculator;
            _electricityReadingsState = electricityReadingsState;
        }

        [EffectMethod]
        public async Task ReloadElectricityReadings(ElectricityReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Electricity);
            dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAction(basicReadings));
            dispatcher.Dispatch(new NotifyElectricityStoreReady());
        }

        [EffectMethod]
        public async Task UpdateElectricityReadings(ElectricityUpdateReadingsAction updateReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromDate(MeterType.Electricity, updateReadingsAction.LastReading);
            dispatcher.Dispatch(new ElectricityStoreUpdatedReadingsAction(basicReadings));
            dispatcher.Dispatch(new NotifyElectricityStoreReady());

        }

        [EffectMethod]
        public Task DeleteElectricityReadings(ElectricityInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyElectricityReadingsDeletedAction());
            return Task.CompletedTask;

        }



        [EffectMethod]
        public async Task InitiateCostCalculations(ElectricityInitiateCostCalculationsAction initiateCostCalculationsAction, IDispatcher dispatcher)
        {
            ImmutableList<CostedReading> costedReadings;
            try
            {
                costedReadings = await Task.Run(() => _energyCostCalculator
                                .GetCostReadings(_electricityReadingsState.Value.BasicReadings,
                                                _electricityTariffsState.Value.TariffDetails));

                dispatcher.Dispatch(new ElectricityStoreCostedReadingsAction(costedReadings));
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction());

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction(failed: true));
            }

        }

    }

    public class NotifyElectricityCostsCalculationFailedAction
    {
        public NotifyElectricityCostsCalculationFailedAction()
        {
        }
    }

    public class NotifyElectricityCostsCalculationCompletedAction
    {
        public bool Failed { get; }

        public NotifyElectricityCostsCalculationCompletedAction(bool failed = false)
        {
            Failed = failed;
        }
    }

    public class ElectricityStoreCostedReadingsAction
    {
        public ImmutableList<CostedReading> CostedReadings { get; }

        public ElectricityStoreCostedReadingsAction(ImmutableList<CostedReading> costedReadings)
        {
            CostedReadings = costedReadings;
        }
    }
}
