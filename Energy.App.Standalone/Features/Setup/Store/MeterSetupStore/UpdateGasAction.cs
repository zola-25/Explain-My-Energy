using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Energy.App.Standalone.Models;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

public class UpdateGasAction
{
    public Meter Meter { get; }

    public UpdateGasAction(Meter meter)
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