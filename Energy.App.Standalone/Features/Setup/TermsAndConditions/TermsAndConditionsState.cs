using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.TermsAndConditions;

[FeatureState(Name = nameof(TermsAndConditionsState))]
[PersistState, PriorityLoad(0)]
public record TermsAndConditionsState
{
    public bool WelcomeScreenSeenAndDismissed { get; init; }


    public TermsAndConditionsState()
    {
        WelcomeScreenSeenAndDismissed = false;
    }
}
