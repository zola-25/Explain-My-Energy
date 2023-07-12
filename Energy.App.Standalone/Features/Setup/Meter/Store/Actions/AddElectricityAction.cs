using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class AddElectricityAction
{
    public Standalone.DTOs.Meter Meter { get; }

    public AddElectricityAction(Standalone.DTOs.Meter meter)
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