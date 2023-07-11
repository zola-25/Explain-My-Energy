//using Energy.App.Standalone.Extensions;
//using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
//using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;
//using Energy.App.Standalone.Models.Tariffs;
//using Energy.Shared;
//using Fluxor;
//using Fluxor.Persist.Storage;
//using System.Collections.Immutable;

//namespace Energy.App.Standalone.Features.Setup.Store
//{
//    [PersistState, PriorityLoad]

//    public record ElectricityTariffsState
//    {
//        public ImmutableList<TariffDetailState> TariffDetails { get; init; }

//    }

//    public class ElectricityTariffsFeature : Feature<ElectricityTariffsState>
//    {
//        public override string GetName()
//        {
//            return nameof(ElectricityTariffsFeature);
//        }

//        protected override ElectricityTariffsState GetInitialState()
//        {
//            return new ElectricityTariffsState
//            {
//                TariffDetails = ImmutableList<TariffDetailState>.Empty
//            };
//        }
//    }

//    public class ElectricityInitiateSetDefaultTariffsAction { }

//    public class ElectricityExecuteSetDefaultTariffsAction
//    {
//        public List<TariffDetailState> DefaultElectricityTariffs { get; }

//        public ElectricityExecuteSetDefaultTariffsAction(List<TariffDetailState> defaultElectricityTarifs)
//        {
//            DefaultElectricityTariffs = defaultElectricityTarifs;
//        }
//    }

//    //public class ElectricityAddTariffAction
//    //{

//    //    public TariffDetail TariffDetail { get; }

//    //    public ElectricityAddTariffAction(TariffDetail tariffDetail)
//    //    {
//    //        TariffDetail = tariffDetail;
//    //    }
//    //}

//    public class ElectricityStoreNewTariffAction
//    {

//        public TariffDetailState TariffDetailState { get; }

//        public ElectricityStoreNewTariffAction(TariffDetailState tariffDetailState)
//        {
//            TariffDetailState = tariffDetailState;
//        }
//    }

//    public class ElectricityUpdateTariffAction
//    {

//        public TariffDetail TariffDetail { get; }

//        public ElectricityUpdateTariffAction(TariffDetail tariffDetail)
//        {
//            TariffDetail = tariffDetail;
//        }
//    }

//    public class ElectricityStoreUpdatedTariffAction
//    {

//        public TariffDetailState TariffDetailState { get; }

//        public ElectricityStoreUpdatedTariffAction(TariffDetailState tariffDetailState)
//        {
//            TariffDetailState = tariffDetailState;
//        }
//    }

//    public class NotifyElectricityTariffsUpdated { }

//    public class DeleteAllElectricityTariffsAction { }


//    public static class ElectricityTariffsReducers
//    {
//        [ReducerMethod]
//        public static ElectricityTariffsState OnExecuteSetDefaultTariffsAction(ElectricityTariffsState state, ElectricityExecuteSetDefaultTariffsAction action)
//        {
//            return state with
//            {
//                TariffDetails = state.TariffDetails.AddRange(action.DefaultElectricityTariffs),
//            };
//        }


//        [ReducerMethod]
//        public static ElectricityTariffsState OnStoreNewTarrifAction(ElectricityTariffsState state, ElectricityStoreNewTariffAction action)
//        {
//            return state with
//            {
//                TariffDetails = state.TariffDetails.Add(action.TariffDetailState)
//            };
//        }

//        [ReducerMethod]
//        public static ElectricityTariffsState OnStoreUpdatedTarrifAction(ElectricityTariffsState state, ElectricityStoreUpdatedTariffAction action)
//        {
//            int index = state.TariffDetails.FindIndex(c => c.GlobalId == action.TariffDetailState.GlobalId);
//            return state with
//            {
//                TariffDetails = state.TariffDetails.SetItem(index, action.TariffDetailState)
//            };
//        }

//        [ReducerMethod]
//        public static ElectricityTariffsState OnDeleteAllTariffs(ElectricityTariffsState state, DeleteAllElectricityTariffsAction action)
//        {
//            return state with
//            {
//                TariffDetails = state.TariffDetails.Clear()
//            };
//        }
//    }

//    // Delete readings with subscribe
//    // Tariffs
//    // Weather Data status

//    public class ElectricityTariffsEffects
//    {
//        [EffectMethod]
//        public Task ExecuteSetDefaultElectricityTariffs(ElectricityInitiateSetDefaultTariffsAction initiateSetDefaultTariffsAction, IDispatcher dispatcher)
//        {
//            List<TariffDetailState> defaultTariffs = DefaultTariffData.DefaultTariffs.Where(c => c.ExampleTariffType == ExampleTariffType.StandardFixedDaily
//                && c.MeterType == MeterType.Electricity).Select(c => new TariffDetailState
//                {
//                    GlobalId = Guid.NewGuid(),
//                    PencePerKWh = c.PencePerKWh,
//                    DailyStandingChargePence = c.DailyStandingChargePence,
//                    DateAppliesFrom = c.DateAppliesFrom,
//                    IsHourOfDayFixed = c.IsHourOfDayFixed,
//                    HourOfDayPrices = c.DefaultHourOfDayPrices.Select(h => new HourOfDayPriceState()
//                    {
//                        HourOfDay = h.HourOfDay,
//                        PencePerKWh = h.PencePerKWh
//                    }).ToImmutableList(),
//                }).ToList();

//            dispatcher.Dispatch(new ElectricityExecuteSetDefaultTariffsAction(defaultTariffs));
//            return Task.CompletedTask;

//        }

//        [EffectMethod]
//        public Task AddElectricityTariff(ElectricityAddTariffAction addTariffAction, IDispatcher dispatcher)
//        {
//            TariffDetailState tariffState = addTariffAction.TariffDetail.eMapToTariffState(addGuidForNewTariff: true);


//            dispatcher.Dispatch(new ElectricityStoreNewTariffAction(tariffState));
//            dispatcher.Dispatch(new NotifyElectricityTariffsUpdated());
//            return Task.CompletedTask;

//        }

//        [EffectMethod]
//        public Task UpdateElectricityTariff(ElectricityUpdateTariffAction updateTariffAction, IDispatcher dispatcher)
//        {
//            TariffDetailState tariffState = updateTariffAction.TariffDetail.eMapToTariffState(addGuidForNewTariff: false);

//            dispatcher.Dispatch(new ElectricityStoreUpdatedTariffAction(tariffState));

//            dispatcher.Dispatch(new NotifyElectricityTariffsUpdated());
//            return Task.CompletedTask;

//        }

//    }


//}
