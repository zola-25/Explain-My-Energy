using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class PreloadMeterSetupStateAction
{
    public DemoMeterSetup DemoMeterSetup { get; private set; }

    public PreloadMeterSetupStateAction(DemoMeterSetup demoMeterSetup)
    {
        DemoMeterSetup = demoMeterSetup;
    }

    [ReducerMethod]
    public static MeterSetupState OnPreloadMeterSetupStateReducer(MeterSetupState state, PreloadMeterSetupStateAction action)
    {
        
        return state with {
            ElectricityMeter = new MeterState {
                Authorized = true,
                AuthorizeFailed = false,
                Authorizing = false,
                GlobalId = action.DemoMeterSetup.ElectricityMeter.GlobalId,
                MeterType = action.DemoMeterSetup.ElectricityMeter.MeterType,
                Mpxn = action.DemoMeterSetup.ElectricityMeter.Mpxn,
                InitialSetupValid = true,
                AuthorizeFailedMessage = null,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Electricity, ExampleTariffType.StandardFixedDaily)
            },
            GasMeter = new MeterState {
                Authorized = true,
                AuthorizeFailed = false,
                Authorizing = false,
                GlobalId = action.DemoMeterSetup.GasMeter.GlobalId,
                MeterType = action.DemoMeterSetup.GasMeter.MeterType,
                Mpxn = action.DemoMeterSetup.GasMeter.Mpxn,
                InitialSetupValid = true,
                AuthorizeFailedMessage = null,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Gas, ExampleTariffType.StandardFixedDaily)
            },
        };
    }
}
