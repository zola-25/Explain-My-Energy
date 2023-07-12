using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions
{
    public class DeleteGasAction
    {

        [ReducerMethod]
        public static MeterSetupState OnGasMeterDeleteReducer(MeterSetupState meterSetupState, DeleteGasAction deleteAction)
        {
            MeterState resettedMeterState = Utilities.GetMeterInitialState(MeterType.Gas);
            return meterSetupState with
            {
                GasMeter = resettedMeterState
            };
        }

        private class Effects : Effect<DeleteGasAction>
        {
            public override async Task HandleAsync(DeleteGasAction action, IDispatcher dispatcher)
            {
                dispatcher.Dispatch(new GasInitiateDeleteReadingsAction());
                dispatcher.Dispatch(new DeleteAllGasTariffsAction());
            }
        }
    }


}
