using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdRollbackLockDataAction
{
    public string OutCodeCharacters { get; init; }
    public string IhdMacId { get; init; }

    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdRollbackLockDataAction action)
    {
        return state with
        {
            OutCodeCharacters = action.OutCodeCharacters,
            IhdMacId = action.IhdMacId
        };
    }
}
