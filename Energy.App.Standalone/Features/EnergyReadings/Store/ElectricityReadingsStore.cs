using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Store
{
    [PersistState]
    public record ElectricityReadingsState
    {
        public bool Reloading { get; init; }
        public bool Updating { get; init; }

        public ImmutableList<BasicReading> BasicReadings { get; init; }
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
                Reloading = false,
                Updating = false,
                BasicReadings = ImmutableList<BasicReading>.Empty
            };
        }
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
                Reloading = false
            };
        }


        [ReducerMethod(typeof(ElectricityReloadReadingsAction))]
        public static ElectricityReadingsState OnBeginReloadReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                Reloading = true
            };
        }

        [ReducerMethod(typeof(ElectricityUpdateReadingsAction))]
        public static ElectricityReadingsState OnBeginUpdateReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                Updating = true
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreUpdatedReadingsReducer(ElectricityReadingsState state, ElectricityStoreUpdatedReadingsAction action)
        {
            return state with
            {
                BasicReadings = state.BasicReadings.AddRange(action.NewReadings),
                Updating = false
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreInitiateDeleteReadingsReducer(ElectricityReadingsState state, ElectricityInitiateDeleteReadingsAction action)
        {
            return state with
            {
                Updating = true
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreExecuteDeleteReadingsReducer(ElectricityReadingsState state, ElectricityExecuteDeleteReadingsAction action)
        {
            return state with
            {
                BasicReadings = ImmutableList<BasicReading>.Empty,
                Updating = false
            };
        }

    }

    public class ElectricityReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;

        public ElectricityReadingsEffects(IEnergyReadingImporter energyReadingImporter)
        {
            _energyReadingImporter = energyReadingImporter;
        }

        [EffectMethod]
        public async Task ReloadElectricityReadings(ElectricityReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveIn(MeterType.Electricity);
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
    }

}
