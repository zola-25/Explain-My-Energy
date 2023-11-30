using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class StoreLockedMeterDataAction
{
    public string GasMeterMprn { get; init; }
    
    public bool GasMeterIdLocked { get; init; }
    public string ElectricityMeterMpan { get; init; }
    public bool ElectricityMeterIdLocked { get; init; }


    [ReducerMethod]
    public static MeterSetupState OnStoreLockedMeterDataReducer(MeterSetupState state, StoreLockedMeterDataAction action)
    {
        string gasMeterMprn = action.GasMeterIdLocked ? action.GasMeterMprn : state.GasMeter.Mpxn;
        string electricityMeterMpan = action.ElectricityMeterIdLocked ? action.ElectricityMeterMpan : state.ElectricityMeter.Mpxn;
        return state with
        {
            GasMeter = state.GasMeter with
            {
                Mpxn = gasMeterMprn,
                MpxnLocked = action.GasMeterIdLocked
            },
            ElectricityMeter = state.ElectricityMeter with
            {
                Mpxn = electricityMeterMpan,
                MpxnLocked = action.ElectricityMeterIdLocked
            }
        };
    }
}
