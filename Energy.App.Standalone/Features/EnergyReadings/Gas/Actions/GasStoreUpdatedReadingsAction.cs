using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreUpdatedReadingsAction
    {
        public ImmutableList<BasicReading> BasicReadings { get; }

        public GasStoreUpdatedReadingsAction(ImmutableList<BasicReading> basicReadings)
        {
            BasicReadings = basicReadings;
        }


    }

    public class GasStoreUpdatedCostsAction
    {
        public ImmutableList<CostedReading> NewCostedReadings { get; }

        public GasStoreUpdatedCostsAction(ImmutableList<CostedReading> newReadings)
        {
            NewCostedReadings = newReadings;
        }
    }

}