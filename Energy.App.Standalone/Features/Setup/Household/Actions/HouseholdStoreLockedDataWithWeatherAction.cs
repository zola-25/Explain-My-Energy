using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdStoreLockedDataWithWeatherAction
{
    public string OutCodeCharacters { get; init; }
    public string IhdMacId { get; init; }
    public bool OutCodeLocked { get; init; }
    public bool IhdMacIdLocked { get; init; }


    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdStoreLockedDataWithWeatherAction action)
    {
        var outCode = action.OutCodeLocked ? action.OutCodeCharacters : state.OutCodeCharacters;
        var ihdMacId = action.IhdMacIdLocked ? action.IhdMacId : state.IhdMacId;
        return state with
        {
            OutCodeCharacters = outCode,
            OutCodeLocked = action.OutCodeLocked,
            IhdMacId = ihdMacId,
            IhdMacIdLocked = action.IhdMacIdLocked
        };
    }

    public class LockedHouseholdEffects : Effect<HouseholdStoreLockedDataWithWeatherAction>
    {
        public override Task HandleAsync(HouseholdStoreLockedDataWithWeatherAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new StoreLockedWeatherOutcodeAction() { OutCode = action.OutCodeCharacters, OutCodeLocked = action.OutCodeLocked });
            return Task.CompletedTask;
        }
    }
}
