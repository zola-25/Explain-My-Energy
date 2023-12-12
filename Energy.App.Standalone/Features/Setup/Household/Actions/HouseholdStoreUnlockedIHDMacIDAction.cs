using Energy.App.Standalone.Features.Setup.Household;
using Fluxor;

public class HouseholdStoreUnlockedIhdMacIdAction
{
    public string IHDMacIDCharacters { get; init; }

    [ReducerMethod] 
    public static HouseholdState OnStoreIHDMacIDReducer(HouseholdState state, HouseholdStoreUnlockedIhdMacIdAction action)
    {
        return state with
        {
            IhdMacId = action.IHDMacIDCharacters
        };
    }
}