using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Energy.App.Standalone.Models;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

public class UpdateElectricityAction
{
    public Meter Meter { get; }


    public UpdateElectricityAction(Meter meter)
    {
        Meter = meter;
    }

    [ReducerMethod]
    public static MeterSetupState Update(MeterSetupState meterSetupState, UpdateElectricityAction updateSuccessAction)
    {
        MeterState meterState = meterSetupState.ElectricityMeter;
        return meterSetupState with
        {
            ElectricityMeter = meterState with
            {
                InitialSetupValid = true,
                Authorized = false,
                MeterType = MeterType.Electricity,
                Mpxn = updateSuccessAction.Meter.Mpxn,

            }
        };

    }
}