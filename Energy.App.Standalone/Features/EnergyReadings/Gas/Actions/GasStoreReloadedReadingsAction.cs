using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasStoreReloadedReadingsAction
    {
        public ImmutableList<CostedReading> CostedReadings { get; }

        public GasStoreReloadedReadingsAction(ImmutableList<CostedReading> costedReadings)
        {
            CostedReadings = costedReadings;
        }
    }

}