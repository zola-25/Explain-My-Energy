using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Meter = Energy.App.Standalone.DTOs.Meter;
using Energy.Shared;
using Fluxor;
using Energy.App.Standalone.DTOs.Tariffs;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class AddGasAction
{
    public Standalone.DTOs.Meter Meter { get; }

    public AddGasAction(Standalone.DTOs.Meter meter)
    {
        Meter = meter;
    }

    [ReducerMethod]
    public static MeterSetupState OnGasMeterInitialAddReducer(MeterSetupState meterSetupState, AddGasAction addSuccessAction)
    {
        MeterState meterState = meterSetupState.GasMeter;
        return meterSetupState with
        {
            GasMeter = meterState with
            {
                GlobalId = Guid.NewGuid(),
                InitialSetupValid = true,
                Authorized = false,
                Mpxn = addSuccessAction.Meter.Mpxn,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Gas, ExampleTariffType.StandardFixedDaily)
            }
        };

    }
}