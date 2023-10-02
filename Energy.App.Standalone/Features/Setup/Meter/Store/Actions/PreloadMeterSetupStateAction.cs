using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class PreloadMeterSetupStateAction
{
    public MeterSetupState MeterSetupState { get; private set; }

    public PreloadMeterSetupStateAction(MeterSetupState meterSetupState)
    {
        MeterSetupState = meterSetupState;
    }

    [ReducerMethod]
    public static MeterSetupState OnPreloadMeterSetupStateReducer(MeterSetupState state, PreloadMeterSetupStateAction action)
    {
        state = action.MeterSetupState;
        return state;
    }
}
