using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreUpdatedReadingsAction
    {
        public ImmutableList<CostedReading> NewCostedReadings { get; }

        public GasStoreUpdatedReadingsAction(ImmutableList<CostedReading> newReadings)
        {
            NewCostedReadings = newReadings;
        }


    }

}