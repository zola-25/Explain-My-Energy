using Energy.App.Standalone.Features.Setup.Meter.Store;
using Fluxor;

public class StoreUnlockedGasMprnAction {
    public string GasMeterMprn { get; init; }

    [ReducerMethod]
    public static MeterSetupState OnStoreUnlockedGasMprnReducer(MeterSetupState state, StoreUnlockedGasMprnAction action)
    {
        return state with
        {
            GasMeter = state.GasMeter with
            {
                Mpxn = action.GasMeterMprn,
            }
        };
    }
}