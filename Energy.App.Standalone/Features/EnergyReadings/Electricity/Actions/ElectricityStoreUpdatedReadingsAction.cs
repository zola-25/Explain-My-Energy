using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityStoreUpdatedReadingsAction
    {
        public ImmutableList<BasicReading> BasicReadings { get; }


        public ElectricityStoreUpdatedReadingsAction(
            ImmutableList<BasicReading> basicReadings)
        {
            BasicReadings = basicReadings;
        }
    }

    public class ElectricityStoreUpdatedCostsAction
    {
        public ImmutableList<CostedReading> NewCostedReadings { get; }

        public ElectricityStoreUpdatedCostsAction(ImmutableList<CostedReading> newCostedReadings)
        {
            NewCostedReadings = newCostedReadings;
        }
    }


}
