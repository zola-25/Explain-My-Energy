using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
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
            private readonly IState<HouseholdState> _householdState;

            public Effects(IState<HouseholdState> householdState)
            {
                _householdState = householdState;

            }
            public override Task HandleAsync(DeleteGasAction action, IDispatcher dispatcher)
            {
                dispatcher.Dispatch(new GasDeleteReadingsAction());
                dispatcher.Dispatch(new DeleteGasHistoricalForecastAction());
                if (_householdState.Value.PrimaryHeatSource == MeterType.Gas)
                {
                    dispatcher.Dispatch(new DeleteHeatingForecastAction(MeterType.Gas));
                }
                return Task.CompletedTask;
            }
        }
    }


}
