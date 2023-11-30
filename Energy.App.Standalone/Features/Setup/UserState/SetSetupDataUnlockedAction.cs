using Fluxor;

namespace Energy.App.Standalone.Features.Setup.UserState;

public class SetSetupDataUnlockedAction
{

    [ReducerMethod(typeof(SetSetupDataUnlockedAction))]
    public static UserLockState Reduce(UserLockState state)
    {
        return state with { 
            Locking = false,
            Unlocking = false,
            SetupDataLocked = false 
        };
    }
}
