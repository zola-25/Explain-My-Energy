using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdStoreUnlockedDataAction
{
    public string OutCodeCharacters { get; init; }
    public string IhdMacId { get; init; }

    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdStoreUnlockedDataAction action)
    {
        return state with
        {
            OutCodeLocked = false,
            IhdMacIdLocked = false,
            OutCodeCharacters = action.OutCodeCharacters,
            IhdMacId = action.IhdMacId
        };
    }
}
