using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdSubmitSuccessAction
{
    public DateTime? MoveInDate { get; }

    public string IhdMacId { get; }
    public string OutCodeCharacters { get; }

    public MeterType PrimaryHeatSource { get; }

    public HouseholdSubmitSuccessAction(DateTime? moveInDate, string ihdMacId, string outCodeCharacters, MeterType primaryHeatSource)
    {
        MoveInDate = moveInDate;
        IhdMacId = ihdMacId;
        OutCodeCharacters = outCodeCharacters;
        PrimaryHeatSource = primaryHeatSource;
    }

    [ReducerMethod]

    public static HouseholdState OnSubmitSuccessReducer(HouseholdState state, HouseholdSubmitSuccessAction action)
    {
        return state with
        {
            Invalid = false,
            Saved = true,
            OutCodeCharacters = action.OutCodeCharacters,
            IhdMacId = action.IhdMacId,
            MoveInDate = action.MoveInDate,
            PrimaryHeatSource = action.PrimaryHeatSource
        };
    }

    private class HouseholdSubmitSuccessEffect : Effect<HouseholdSubmitSuccessAction>
    {
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<HeatingForecastState> _heatingForecastState;
        private readonly IState<MeterSetupState> _meterSetupState;

        public HouseholdSubmitSuccessEffect(IState<HouseholdState> householdState, IState<WeatherState> weatherState, IState<HeatingForecastState> heatingForecastState, IState<MeterSetupState> meterSetupState)
        {
            _householdState = householdState;
            _weatherState = weatherState;
            _heatingForecastState = heatingForecastState;
            _meterSetupState = meterSetupState;
        }

        public override Task HandleAsync(HouseholdSubmitSuccessAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyHouseholdSavedSuccess());

            bool outCodeCharactersChanged = action.OutCodeCharacters != _weatherState.Value.OC;

            bool newHeatingMeterValid = _meterSetupState.Value[action.PrimaryHeatSource].SetupValid;

            bool primaryHeatSourceChanged = _heatingForecastState.Value.HeatingMeterType != action.PrimaryHeatSource;
            bool updateHeatingForecast = primaryHeatSourceChanged || !_heatingForecastState.Value.SavedCoefficients;


            dispatcher.Dispatch(new HandleHouseholdChangesAction(
                updateWeatherData: outCodeCharactersChanged,
                resetHeatingForecast: !newHeatingMeterValid,
                updateHeatingForecast: updateHeatingForecast,
                primaryHeatSource: action.PrimaryHeatSource));

            return Task.CompletedTask;
        }
    }
}
