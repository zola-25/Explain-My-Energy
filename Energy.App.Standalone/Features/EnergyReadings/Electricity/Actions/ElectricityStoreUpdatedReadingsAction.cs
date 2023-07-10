using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityStoreUpdatedReadingsAction
    {
        public ImmutableList<CostedReading> NewCostedReadings { get; }

        public ElectricityStoreUpdatedReadingsAction(ImmutableList<CostedReading> newCostedReadings)
        {
            NewCostedReadings = newCostedReadings;
        }
    }


}
