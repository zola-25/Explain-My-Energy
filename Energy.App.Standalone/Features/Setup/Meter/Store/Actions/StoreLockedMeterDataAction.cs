using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class StoreLockedMeterDataAction
{
    public string GasMeterMprn { get; init; }
    
    public string ElectricityMeterMpan { get; init; }


    [ReducerMethod]
    public static MeterSetupState OnStoreLockedMeterDataReducer(MeterSetupState state, StoreLockedMeterDataAction action)
    {
        return state with
        {
            GasMeter = state.GasMeter with
            {
                Mpxn = action.GasMeterMprn,
            },
            ElectricityMeter = state.ElectricityMeter with
            {
                Mpxn = action.ElectricityMeterMpan,
            }
        };
    }
}
