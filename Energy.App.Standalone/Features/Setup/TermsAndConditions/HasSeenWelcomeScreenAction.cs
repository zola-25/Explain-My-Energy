using Fluxor;

namespace Energy.App.Standalone.Features.Setup.TermsAndConditions;

// create action class 
public class HasSeenWelcomeScreenAction
{
    public bool Seen { get; }

    public HasSeenWelcomeScreenAction(bool seen)
    {
        Seen = seen;
    }

    [ReducerMethod]
    public static TermsAndConditionsState Reduce(TermsAndConditionsState state, HasSeenWelcomeScreenAction action)
    {
        return state with { WelcomeScreenSeenAndDismissed = action.Seen };
    }
}
