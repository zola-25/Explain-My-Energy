using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class StorePreloadedElectricityReadingsAction
{

    public ImmutableList<BasicReading> DemoElectricityReadings { get; init; }

    public StorePreloadedElectricityReadingsAction(ImmutableList<BasicReading> demoElectricityReadings)
    {
        DemoElectricityReadings = demoElectricityReadings;
    }

    [ReducerMethod]
    public static ElectricityReadingsState OnPreloadElectricityReadingsReducer(ElectricityReadingsState state, StorePreloadedElectricityReadingsAction action)
    {
        return state with { 
            LastCheckedForNewReadings = action.DemoElectricityReadings.Last().UtcTime,
            Loading = false,
            BasicReadings = action.DemoElectricityReadings 
        };
    }
}
