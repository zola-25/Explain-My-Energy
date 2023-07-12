using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class UpdateGasAction
{
    public Standalone.DTOs.Meter Meter { get; }

    public UpdateGasAction(Standalone.DTOs.Meter meter)
    {
        Meter = meter;
    }


    [ReducerMethod]
    public static MeterSetupState Update(MeterSetupState meterSetupState, UpdateGasAction updateSuccessAction)
    {
        MeterState meterState = meterSetupState.GasMeter;
        return meterSetupState with
        {
            GasMeter = meterState with
            {
                InitialSetupValid = true,
                Authorized = false,
                Mpxn = updateSuccessAction.Meter.Mpxn
            }
        };
    }

}