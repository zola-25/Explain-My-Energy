using Fluxor;

namespace Energy.App.Standalone.Features.Setup.UserState;

public class SetSetupDataBeginLockingAction
{

    [ReducerMethod(typeof(SetSetupDataBeginLockingAction))]
    public static UserLockState Reduce(UserLockState state)
    {
        return state with { Locking = true };
    }
}
