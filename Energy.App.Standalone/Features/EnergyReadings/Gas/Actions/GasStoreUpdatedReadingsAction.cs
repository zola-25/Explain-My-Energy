using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreUpdatedReadingsAction
    {
        public ImmutableList<BasicReading> BasicReadings { get; }
        public ImmutableList<CstR> NewCostedReadings { get; }

        public GasStoreUpdatedReadingsAction(ImmutableList<BasicReading> basicReadings, ImmutableList<CstR> newReadings)
        {
            BasicReadings = basicReadings;
            NewCostedReadings = newReadings;
        }


    }

}