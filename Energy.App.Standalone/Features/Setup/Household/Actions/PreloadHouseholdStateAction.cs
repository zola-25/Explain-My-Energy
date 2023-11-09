using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class PreloadHouseholdStateAction
{
    public DemoHousehold DemoHousehold { get;  }

    public PreloadHouseholdStateAction(DemoHousehold demoHousehold)
    {
        DemoHousehold = demoHousehold;
    }

    [ReducerMethod]

    public static HouseholdState OnSubmitSuccessReducer(HouseholdState state, PreloadHouseholdStateAction action)
    {
        return state with {
            IhdMacId = action.DemoHousehold.IhdMacId,
            Invalid = false,
            MoveInDate = action.DemoHousehold.MoveInDate,
            OutCodeCharacters = action.DemoHousehold.OutCodeCharacters,
            PrimaryHeatSource = action.DemoHousehold.PrimaryHeatSource,
            Saved = true,
        };
    }
}
