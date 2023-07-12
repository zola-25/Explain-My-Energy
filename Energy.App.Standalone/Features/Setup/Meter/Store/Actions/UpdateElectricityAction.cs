using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class UpdateElectricityAction
{
    public Standalone.DTOs.Meter Meter { get; }


    public UpdateElectricityAction(Standalone.DTOs.Meter meter)
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