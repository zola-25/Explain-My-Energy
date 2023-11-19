using Fluxor;

namespace Energy.App.Standalone.Features.Setup.UserState;

public class SetSetupDataUnlockedAction
{

    [ReducerMethod(typeof(SetSetupDataUnlockedAction))]
    public static UserLockState Reduce(UserLockState state)
    {
        return state with { 
            Unlocking = false,
            SetupDataLocked = false 
        };
    }
}
