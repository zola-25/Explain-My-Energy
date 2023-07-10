using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    public static class GasReadingsReducers
    {
        [ReducerMethod]
        public static GasReadingsState OnStoreReloadedReadingsReducer(GasReadingsState state, GasStoreReloadedReadingsAction action)
        {
            return state with
            {
                BasicReadings = action.BasicReadings,
                CostedReadings = action.CostedReadings,
                ReloadingReadings = false,
                UpdatingReadings = false,
                CalculationError = false,
                LastUpdated = DateTime.UtcNow
            };
        }


        [ReducerMethod(typeof(GasReloadReadingsAndCostsAction))]
        public static GasReadingsState OnBeginReloadReadingsReducer(GasReadingsState state)
        {
            return state with
            {
                ReloadingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod(typeof(GasUpdateReadingsAndCostsAction))]
        public static GasReadingsState OnBeginUpdateReadingsReducer(GasReadingsState state)
        {
            return state with
            {
                UpdatingReadings = true,
                CalculationError = false
            };
        }

        [ReducerMethod]
        public static GasReadingsState OnStoreUpdatedReadingsReducer(GasReadingsState state, GasStoreUpdatedReadingsAction action)
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
                CostedReadings = ImmutableList<CstR>.Empty,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                ReloadingReadings = false,
                UpdatingReadings = false,
                LastUpdated = DateTime.MinValue,
                CalculationError = false

            };
        }



        [ReducerMethod]
        public static GasReadingsState OnCostCalculationsFailedNotification(GasReadingsState state, NotifyGasCostsCalculationFailedAction action)
        {
            return state with
            {
                CostedReadings = ImmutableList<CstR>.Empty,
                ReloadingReadings = false,
                UpdatingReadings = false,
                CalculationError = true,
                LastUpdated = DateTime.MinValue
            };
        }
    }
}