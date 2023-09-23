using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class PreloadGasReadingsAction
{
    public GasReadingsState GasReadingsState { get; }

    public PreloadGasReadingsAction(GasReadingsState gasReadingsState)
    {
        GasReadingsState = gasReadingsState;
    }

    [ReducerMethod]
    public static GasReadingsState OnPreloadGasReadingsReducer(GasReadingsState state, PreloadGasReadingsAction action)
    {
        state = action.GasReadingsState;
        return state;
    }
}
