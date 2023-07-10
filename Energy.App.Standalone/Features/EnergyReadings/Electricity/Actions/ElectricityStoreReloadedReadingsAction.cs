using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityStoreReloadedReadingsAction
    {
        public ImmutableList<CstR> CostedReadings { get; }

        public ElectricityStoreReloadedReadingsAction(ImmutableList<CstR> costedReadings)
        {
            CostedReadings = costedReadings;
        }
    }


}
