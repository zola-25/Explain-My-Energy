using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class PreloadHouseholdStateAction
{
    public HouseholdState HouseholdState { get; }

    public PreloadHouseholdStateAction(HouseholdState householdState)
    {
        HouseholdState = householdState;
    }


    [ReducerMethod]

    public static HouseholdState OnSubmitSuccessReducer(HouseholdState state, PreloadHouseholdStateAction action)
    {
        state = action.HouseholdState;
        return state;
    }
}
