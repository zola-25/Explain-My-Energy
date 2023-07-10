using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreReloadedReadingsAction
    {
        public ImmutableList<CstR> CostedReadings { get; }
        public ImmutableList<BasicReading> BasicReadings { get; }

        public GasStoreReloadedReadingsAction(ImmutableList<BasicReading> basicReadings, ImmutableList<CstR> costedReadings)
        {
            CostedReadings = costedReadings;
            BasicReadings = basicReadings;
        }
    }

}