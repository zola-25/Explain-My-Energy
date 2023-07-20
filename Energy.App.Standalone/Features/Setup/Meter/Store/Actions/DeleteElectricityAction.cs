using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class DeleteElectricityAction
{

    [ReducerMethod(typeof(DeleteElectricityAction))]
    public static MeterSetupState OnElectricityMeterDeleteReducer(MeterSetupState meterSetupState)
    {
        MeterState resettedMeterState = Utilities.GetMeterInitialState(MeterType.Electricity);
        return meterSetupState with
        {
            ElectricityMeter = resettedMeterState
        };
    }

    private class Effects : Effect<DeleteElectricityAction>
    {
        private readonly IState<HouseholdState> _householdState;

        public Effects(IState<HouseholdState> householdState)
        {
            _householdState = householdState;
        }

        public override async Task HandleAsync(DeleteElectricityAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityDeleteReadingsAction());
            dispatcher.Dispatch(new DeleteElectricityHistoricalForecastAction());
            if (_householdState.Value.PrimaryHeatSource == MeterType.Electricity)
            {
                dispatcher.Dispatch(new DeleteHeatingForecastAction(MeterType.Electricity));
            }
        }
    }
}