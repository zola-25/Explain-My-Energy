using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    public static class GasReadingsReducers
    {
        [ReducerMethod]
        public static GasReadingsState OnStoreReloadedReadingsAndCostsReducer(GasReadingsState state, GasStoreReloadedReadingsAndCostsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings,
                CostedReadings = action.CostedReadings,
                ReloadingReadings = false,
                ReloadingCosts = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }

        
        [ReducerMethod]
        public static GasReadingsState OnStoreUpdatedCostedReadingsReducer(GasReadingsState state, GasStoreUpdatedCostsAction action)
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
        public static GasReadingsState OnStoreUpdatedReadingsReducer(GasReadingsState state, GasStoreUpdatedReadingsAction action)
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
        public static GasReadingsState OnStoreReloadedCostsReducer(GasReadingsState state, GasStoreReloadedCostsOnlysAction action)
        {
            return state with
            {
                CostedReadings = action.CostedReadings,
                ReloadingCosts = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }
        

        [ReducerMethod(typeof(GasReloadReadingsAndCostsAction))]
        public static GasReadingsState OnBeginReloadReadingsAndCostsReducer(GasReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                ReloadingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(GasUpdateReadingsAndReloadCostsAction))]
        public static GasReadingsState OnBeginUpdateReadingsAndReloadCostsReducer(GasReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                UpdatingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(GasReloadCostsOnlyAction))]
        public static ElectricityReadingsState OnBeginReloadCostsOnly(ElectricityReadingsState state)
        {
            return state with
            {
                ReloadingCosts = true,
                CalculationError = false
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreInitiateDeleteReadingsReducer(GasReadingsState state, GasInitiateDeleteReadingsAction action)
        {
            return state with
            {
                UpdatingReadings = true
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreExecuteDeleteReadingsReducer(GasReadingsState state, GasExecuteDeleteReadingsAction action)
        {
            return state with
            {
                CostedReadings = ImmutableList<CostedReading>.Empty,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                ReloadingReadings = false,
                UpdatingReadings = false,
                ReloadingCosts = false,
                UpdatingCosts = false,
                LastUpdated = DateTime.MinValue,
                CalculationError = false

            };
        }



        [ReducerMethod]
        public static GasReadingsState OnCostCalculationsFailedNotification(GasReadingsState state, NotifyGasCostsCalculationFailedAction action)
        {
            return state with
            {
                CostedReadings = ImmutableList<CostedReading>.Empty,
                ReloadingReadings = false,
                UpdatingReadings = false,
                ReloadingCosts = false,
                UpdatingCosts = false,
                CalculationError = true,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}