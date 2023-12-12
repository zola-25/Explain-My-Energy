using Fluxor;

namespace Energy.App.Standalone.Features.Setup.UserState;

public class SetSetupDataBeginUnlockingAction
{
    [ReducerMethod(typeof(SetSetupDataBeginUnlockingAction))]
    public static UserLockState Reduce(UserLockState state)
    {
        return state with { Unlocking = true };
    }

}
