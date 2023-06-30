using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.EnergyReadings.Store
{
    [PersistState]
    public record GasReadingsState
    {
        public bool Reloading { get; init; }
        public bool Updating { get; init; }

        public List<BasicReading> BasicReadings { get; init; }
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
                BasicReadings = new List<BasicReading>()
            };
        }
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
                BasicReadings = action.BasicReadings,
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
            List<BasicReading> readingsToUpdate = state.BasicReadings.ToList();
            readingsToUpdate.AddRange(action.NewReadings);
            return state with
            {
                BasicReadings = readingsToUpdate,
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
                BasicReadings = new List<BasicReading>(),
                Updating = false
            };
        }

    }

    // Delete readings with subscribe
    // Tariffs
    // Weather Data status

    public class GasReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;

        public GasReadingsEffects(IEnergyReadingImporter energyReadingImporter)
        {
            _energyReadingImporter = energyReadingImporter;
        }

        [EffectMethod]
        public async Task ReloadGasReadings(GasReloadReadingsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveIn(MeterType.Gas);
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
        public void DeleteGasReadings(GasInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyGasReadingsDeletedAction());
        }
    }

}
