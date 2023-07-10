using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Store
{
    public static class ElectricityReadingsReducers
    {



        [ReducerMethod]
        public static ElectricityReadingsState OnStoreReloadedReadingsReducer(ElectricityReadingsState state, ElectricityStoreReloadedReadingsAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                ReloadingReadings = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }


        [ReducerMethod(typeof(ElectricityReloadReadingsAndCostsAction))]
        public static ElectricityReadingsState OnBeginReloadReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(ElectricityUpdateReadingsAndCostsAction))]
        public static ElectricityReadingsState OnBeginUpdateReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                UpdatingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreUpdatedReadingsReducer(ElectricityReadingsState state, ElectricityStoreUpdatedReadingsAction action)
        {
            var replaceFrom = action.NewCostedReadings.First().UtcTime;
            return state with
            {
                CostedReadings = state.CostedReadings
                    .RemoveAll(c => c.UtcTime >= replaceFrom)
                    .AddRange(action.NewCostedReadings),
                UpdatingReadings = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
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
                CostedReadings = ImmutableList<CostedReading>.Empty,
                UpdatingReadings = false,
                CalculationError = false,
                LastUpdated = DateTime.MinValue
            };
        }




        [ReducerMethod]

        public static ElectricityReadingsState OnCostCalculationsFailedNotification(ElectricityReadingsState state, NotifyElectricityCostsCalculationFailedAction action)
        {
            return state with
            {
                CostedReadings = ImmutableList<CostedReading>.Empty,
                UpdatingReadings = false,
                ReloadingReadings = false,
                CalculationError = true,
                LastUpdated = DateTime.MinValue
            };
        }

    }


}
