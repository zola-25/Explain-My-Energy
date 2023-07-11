using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

public class AddElectricityAction
{
    public Meter Meter { get; }

    public AddElectricityAction(Meter meter)
    {
        Meter = meter;
    }

    [ReducerMethod]
    public static MeterSetupState Add(MeterSetupState meterSetupState, AddElectricityAction addSuccessAction)
    {
        MeterState meterState = meterSetupState.ElectricityMeter;
        return meterSetupState with
        {
            ElectricityMeter = meterState with
            {
                GlobalId = Guid.NewGuid(),
                InitialSetupValid = true,
                Authorized = false,
                MeterType = MeterType.Electricity,
                Mpxn = addSuccessAction.Meter.Mpxn,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Electricity, ExampleTariffType.StandardFixedDaily)
            }
        };

    }
}