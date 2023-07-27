using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.TermsAndConditions;

[FeatureState(Name = nameof(TermsAndConditionsState))]
[PersistState, PriorityLoad]
public record TermsAndConditionsState
{
    public bool Accepted { get; init; }
}

// create action class 
public class AcceptTermsAndConditionsAction
{
    public bool Accepted { get;  }

    public AcceptTermsAndConditionsAction(bool accepted)
    {
        Accepted = accepted;
    }

    [ReducerMethod]
    public static TermsAndConditionsState Reduce(TermsAndConditionsState state, AcceptTermsAndConditionsAction action)
    {
        return state with { Accepted = action.Accepted };
    }

}
