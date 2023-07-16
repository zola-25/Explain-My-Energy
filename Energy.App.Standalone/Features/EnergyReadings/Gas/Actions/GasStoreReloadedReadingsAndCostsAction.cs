using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreReloadedReadingsAndCostsAction
    {
        public ImmutableList<CostedReading> CostedReadings { get; }
        public ImmutableList<BasicReading> BasicReadings { get; }

        public GasStoreReloadedReadingsAndCostsAction(
            ImmutableList<BasicReading> basicReadings,
            ImmutableList<CostedReading> costedReadings
            )
        {
            CostedReadings = costedReadings;
            BasicReadings = basicReadings;
        }
    }
}