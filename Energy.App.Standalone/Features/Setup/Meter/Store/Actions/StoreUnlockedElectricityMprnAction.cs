
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Fluxor;

public class StoreUnlockedElectricityMprnAction {
    public string ElectricityMeterMprn { get; init; }

    [ReducerMethod]
    public static MeterSetupState OnStoreUnlockedElectricityMprnReducer(MeterSetupState state, StoreUnlockedElectricityMprnAction action)
    {
        return state with
        {
            ElectricityMeter = state.ElectricityMeter with
            {
                Mpxn = action.ElectricityMeterMprn,
                MpxnLocked = false
            }
        };
    }
}