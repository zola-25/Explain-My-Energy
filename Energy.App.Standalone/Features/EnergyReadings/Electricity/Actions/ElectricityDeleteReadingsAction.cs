using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityDeleteReadingsAction
    { 
        [ReducerMethod(typeof(ElectricityDeleteReadingsAction))]
        public static ElectricityReadingsState DeleteReadingsReducer(ElectricityReadingsState state)
        {
            return state with
            {
                
                CostedReadings = ImmutableList<CostedReading>.Empty,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                Loading = false,
                BasicReadingLastUpdated = DateTime.MinValue,
                LastCheckedForNewReadings = DateTime.MinValue
            };
        }}


}
