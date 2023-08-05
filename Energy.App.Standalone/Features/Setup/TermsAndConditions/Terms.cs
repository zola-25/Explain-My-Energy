using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.TermsAndConditions;

[FeatureState(Name = nameof(TermsAndConditionsState))]
[PersistState, PriorityLoad]
public record TermsAndConditionsState
{
    public bool WelcomeScreenSeenAndDismissed { get; init; }
}

// create action class 
public class HasSeenWelcomeScreenAction
{
    public bool Seen { get;  }

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
