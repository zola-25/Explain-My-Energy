using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityStoreUpdatedReadingsAction
    {
        public ImmutableList<CstR> NewCostedReadings { get; }

        public ElectricityStoreUpdatedReadingsAction(ImmutableList<CstR> newCostedReadings)
        {
            NewCostedReadings = newCostedReadings;
        }
    }


}
