using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class ElectricityStoreReloadedCostsOnlyAction
{
    public ImmutableList<CostedReading> CostedReadings { get; }

    public ElectricityStoreReloadedCostsOnlyAction(
        ImmutableList<CostedReading> costedReadings)
    {
        CostedReadings = costedReadings;
    }
}
