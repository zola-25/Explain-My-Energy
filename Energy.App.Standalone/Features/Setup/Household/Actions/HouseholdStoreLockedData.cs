using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdStoreLockedData
{
    public string OutCodeCharacters { get; init; }
    public string IhdMacId { get; init; }
    public bool OutCodeLocked { get; init; }
    public bool IhdMacIdLocked { get; init; }


    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdStoreLockedData action)
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
}
