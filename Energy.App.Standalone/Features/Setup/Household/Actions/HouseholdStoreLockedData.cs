using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HouseholdStoreLockedData
{
    public string OutCodeCharacters { get; init; }
    public string IhdMacId { get; init; }


    [ReducerMethod]
    public static HouseholdState OnStoreOutCodeReducer(HouseholdState state, HouseholdStoreLockedData action)
    {
        var outCode = action.OutCodeCharacters;
        var ihdMacId = action.IhdMacId;
        return state with
        {
            OutCodeCharacters = outCode,
            IhdMacId = ihdMacId,
        };
    }
}
