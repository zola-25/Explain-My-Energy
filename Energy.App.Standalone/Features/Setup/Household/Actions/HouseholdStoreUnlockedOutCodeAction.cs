// Action class with outcode property with reducer that sets the household state to unlocked outcode and sets OutCode lock to false

using Energy.App.Standalone.Features.Setup.Household;
using Fluxor;

public class HouseholdStoreUnlockedOutCodeAction
{
    public string OutCodeCharacters { get; init; }

    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdStoreUnlockedOutCodeAction action)
    {
        return state with
        {
            OutCodeCharacters = action.OutCodeCharacters
        };
    }
}


