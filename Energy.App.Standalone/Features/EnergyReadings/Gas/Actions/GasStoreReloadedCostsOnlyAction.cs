using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class GasStoreReloadedCostsOnlyAction
{
    public ImmutableList<CostedReading> CostedReadings { get; }

    public GasStoreReloadedCostsOnlyAction(
        ImmutableList<CostedReading> costedReadings)
    {
        CostedReadings = costedReadings;
    }
}