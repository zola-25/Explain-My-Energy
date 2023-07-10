using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Store
{
    public static class ElectricityReadingsReducers
    {
        [ReducerMethod]
        public static ElectricityReadingsState OnStoreReloadedReadingsAndCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedReadingsAndCostsAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                BasicReadings = action.BasicReadings,
                ReloadingReadings = false,
                ReloadingCosts = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreUpdatedCostsReducer(ElectricityReadingsState state, ElectricityStoreUpdatedCostsAction action)
        {
            var replaceFrom = action.NewCostedReadings.First().UtcTime;
            return state with
            {
                CostedReadings = state.CostedReadings
                    .RemoveAll(c => c.UtcTime >= replaceFrom)
                    .AddRange(action.NewCostedReadings),
                UpdatingCosts = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreUpdatedReadingsReducer(ElectricityReadingsState state, ElectricityStoreUpdatedReadingsAction action)
        {
            var replaceFrom = action.BasicReadings.First().UtcTime;
            return state with
            {
                BasicReadings = state.BasicReadings
                    .RemoveAll(c => c.UtcTime >= replaceFrom)
                    .AddRange(action.BasicReadings),
                UpdatingReadings = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        [ReducerMethod]
        public static ElectricityReadingsState OnStoreReloadedCostsReducer(ElectricityReadingsState state, ElectricityStoreReloadedCostsOnlysAction reloadedCostsOnlysAction)
        {
            return state with
            {
                CostedReadings = reloadedCostsOnlysAction.CostedReadings,
                ReloadingCosts = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        


        [ReducerMethod(typeof(ElectricityReloadReadingsAndCostsAction))]
        public static ElectricityReadingsState OnBeginReloadReadingsAndCostsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                ReloadingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(ElectricityUpdateReadingsAndReloadCostsAction))]
        public static ElectricityReadingsState OnBeginUpdateReadingsAndReloadCostsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                UpdatingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(ElectricityReloadCostsOnlyAction))]
        public static ElectricityReadingsState OnBeginReloadCostsOnly(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                CalculationError = false
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
                ReloadingReadings = false,
                CalculationError = false,
                ReloadingCosts = false,
                UpdatingCosts = false,
                BasicReadings = ImmutableList<BasicReading>.Empty,
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
                UpdatingCosts = false,
                ReloadingCosts = false, 
                ReloadingReadings = false,
                CalculationError = true,
                LastUpdated = DateTime.UtcNow
            };
        }

    }


}
