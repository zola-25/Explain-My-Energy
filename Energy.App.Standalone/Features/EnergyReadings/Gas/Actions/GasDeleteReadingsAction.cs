using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasDeleteReadingsAction
    {
        [ReducerMethod(typeof(GasDeleteReadingsAction))]
        public static GasReadingsState DeleteReadingsReducer(GasReadingsState state)
        {
            return state with
            {
                CostedReadings = ImmutableList<CostedReading>.Empty,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                Loading = false,
                LastUpdated = DateTime.MinValue,
                CalculationError = String.Empty
            };
        }
    }

}