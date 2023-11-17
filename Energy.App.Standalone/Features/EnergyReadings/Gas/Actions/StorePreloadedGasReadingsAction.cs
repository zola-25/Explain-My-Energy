using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class StorePreloadedGasReadingsAction
{

    public ImmutableList<BasicReading> DemoGasReadings { get; init; }

    public StorePreloadedGasReadingsAction(ImmutableList<BasicReading> demoGasReadings)
    {
        DemoGasReadings = demoGasReadings;
    }

    [ReducerMethod]
    public static GasReadingsState OnPreloadGasReadingsReducer(GasReadingsState state, StorePreloadedGasReadingsAction action)
    {
        return state with { 
            LastCheckedForNewReadings = action.DemoGasReadings.Last().Utc,
            Loading = false,
            BasicReadings = action.DemoGasReadings 
        };
    }
}
