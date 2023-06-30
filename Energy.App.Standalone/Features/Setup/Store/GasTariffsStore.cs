using Energy.App.Standalone.Data;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Setup.Store
{
    [PersistState, PriorityLoad]

    public record GasTariffsState
    {
        public ImmutableList<TariffDetailState> TariffDetails { get; init; }

    }

    public class GasTariffsFeature : Feature<GasTariffsState>
    {
        public override string GetName()
        {
            return nameof(GasTariffsFeature);
        }

        protected override GasTariffsState GetInitialState()
        {
            return new GasTariffsState
            {
                TariffDetails = ImmutableList<TariffDetailState>.Empty
            };
        }
    }

    public class GasInitiateSetDefaultTariffsAction { }

    public class GasExecuteSetDefaultTariffsAction
    {
        public List<TariffDetailState> DefaultGasTariffs { get; }

        public GasExecuteSetDefaultTariffsAction(List<TariffDetailState> defaultGasTarifs)
        {
            DefaultGasTariffs = defaultGasTarifs;
        }
    }

    public class GasAddTariffAction {

        public TariffDetail TariffDetail { get;  }

        public GasAddTariffAction(TariffDetail tariffDetail)
        {
            TariffDetail = tariffDetail;
        }
    }

    public class GasStoreNewTariffAction {

        public TariffDetailState TariffDetailState { get;  }

        public GasStoreNewTariffAction(TariffDetailState tariffDetailState)
        {
            TariffDetailState = tariffDetailState;
        }
    }

    public class GasUpdateTariffAction {

        public TariffDetail TariffDetail { get;  }

        public GasUpdateTariffAction(TariffDetail tariffDetail)
        {
            TariffDetail = tariffDetail;
        }
    }

    public class GasStoreUpdatedTariffAction {

        public TariffDetailState TariffDetailState { get;  }

        public GasStoreUpdatedTariffAction(TariffDetailState tariffDetailState)
        {
            TariffDetailState = tariffDetailState;
        }
    }

    public class DeleteAllGasTariffsAction { }

    public class NotifyGasTariffsUpdated { }

    public static class GasTariffsReducers
    {
        [ReducerMethod]
        public static GasTariffsState OnExecuteSetDefaultTariffsAction(GasTariffsState state, GasExecuteSetDefaultTariffsAction action)
        {
            return state with
            {
                TariffDetails = state.TariffDetails.AddRange(action.DefaultGasTariffs),
            };
        }

        [ReducerMethod]
        public static GasTariffsState OnStoreNewTarrifAction(GasTariffsState state, GasStoreNewTariffAction action)
        {
            return state with
            {
                TariffDetails = state.TariffDetails.Add(action.TariffDetailState)
            };
        }

        [ReducerMethod]
        public static GasTariffsState OnStoreUpdatedTarrifAction(GasTariffsState state, GasStoreUpdatedTariffAction action)
        {
            var index = state.TariffDetails.FindIndex(c => c.GlobalId == action.TariffDetailState.GlobalId);
            return state with
            {
                TariffDetails = state.TariffDetails.SetItem(index, action.TariffDetailState)
            };
        }

        [ReducerMethod]
        public static GasTariffsState OnDeleteAllTariffs(GasTariffsState state, DeleteAllGasTariffsAction action)
        {
            return state with
            {
                TariffDetails = state.TariffDetails.Clear()
            };
        }
    }

    // Delete readings with subscribe
    // Tariffs
    // Weather Data status

    public class GasTariffsEffects
    {
        [EffectMethod]
        public Task ExecuteSetDefaultGasTariffs(GasInitiateSetDefaultTariffsAction initiateSetDefaultTariffsAction, IDispatcher dispatcher)
        {
            var defaultTariffs = DefaultTariffData.DefaultTariffs.Where(c => c.ExampleTariffType == ExampleTariffType.StandardFixedDaily
                && c.MeterType == MeterType.Gas).Select(c => new TariffDetailState
                {
                    GlobalId = Guid.NewGuid(),
                    PencePerKWh = c.PencePerKWh,
                    DailyStandingChargePence = c.DailyStandingChargePence,
                    DateAppliesFrom = c.DateAppliesFrom,
                    IsHourOfDayFixed = c.IsHourOfDayFixed,
                    HourOfDayPrices = c.DefaultHourOfDayPrices.Select(h => new HourOfDayPriceState()
                    {
                        HourOfDay = h.HourOfDay,
                        PencePerKWh = h.PencePerKWh
                    }).ToList(),
                }).ToList();

            dispatcher.Dispatch(new GasExecuteSetDefaultTariffsAction(defaultTariffs));
            return Task.CompletedTask;

        }

        [EffectMethod]
        public Task AddGasTariff(GasAddTariffAction addTariffAction, IDispatcher dispatcher)
        {
            var tariffState = addTariffAction.TariffDetail.eMapToTariffState(addGuidForNewTariff: true);
            

            dispatcher.Dispatch(new GasStoreNewTariffAction(tariffState));
            dispatcher.Dispatch(new NotifyGasTariffsUpdated());
            return Task.CompletedTask;

        }

        [EffectMethod]
        public Task UpdateGasTariff(GasUpdateTariffAction updateTariffAction, IDispatcher dispatcher)
        {
            var tariffState = updateTariffAction.TariffDetail.eMapToTariffState(addGuidForNewTariff: false);
            
            dispatcher.Dispatch(new GasStoreUpdatedTariffAction(tariffState));
            dispatcher.Dispatch(new NotifyGasTariffsUpdated());
            return Task.CompletedTask;

        }

    }


}
