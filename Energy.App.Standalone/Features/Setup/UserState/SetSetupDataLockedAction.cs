using Fluxor;

namespace Energy.App.Standalone.Features.Setup.UserState;

public class SetSetupDataLockedAction
{

    [ReducerMethod(typeof(SetSetupDataLockedAction))]
    public static UserLockState Reduce(UserLockState state)
    {
        return state with { 
            Locking = false,
            SetupDataLocked = true };
    }
}
