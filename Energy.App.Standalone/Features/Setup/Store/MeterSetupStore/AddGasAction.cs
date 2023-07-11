using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

public class AddGasAction
{
    public Meter Meter { get; }

    public AddGasAction(Meter meter)
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