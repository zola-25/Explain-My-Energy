using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class MeterDataRollbackLockDataAction
{
    public bool UnlockGasMeter { get; init; }
    public bool UnlockElectricityMeter { get; init; }
    public string GasMeterMprn { get; init; }
    public string ElectricityMeterMpan { get; init; }

    [ReducerMethod]
    public static MeterSetupState OnStoreUnlockedMeterDataReducer(MeterSetupState state, MeterDataRollbackLockDataAction action)
    {
        string gasMeterMprn = action.UnlockGasMeter ? action.GasMeterMprn : state.GasMeter.Mpxn;
        string electricityMeterMpan = action.UnlockElectricityMeter ? action.ElectricityMeterMpan : state.ElectricityMeter.Mpxn;
        return state with
        {
            GasMeter = state.GasMeter with
            {
                Mpxn = gasMeterMprn,
            },
            ElectricityMeter = state.ElectricityMeter with
            {
                Mpxn = electricityMeterMpan,
            }
        };
    }
}
