using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class DeleteElectricityAction
{

    [ReducerMethod]
    public static MeterSetupState OnElectricityMeterDeleteReducer(MeterSetupState meterSetupState, DeleteElectricityAction deleteAction)
    {
        MeterState resettedMeterState = Utilities.GetMeterInitialState(MeterType.Electricity);
        return meterSetupState with
        {
            ElectricityMeter = resettedMeterState
        };
    }

    private class Effects : Effect<DeleteElectricityAction>
    {
        public override async Task HandleAsync(DeleteElectricityAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityInitiateDeleteReadingsAction());
            dispatcher.Dispatch(new DeleteAllElectricityTariffsAction());
        }
    }
}