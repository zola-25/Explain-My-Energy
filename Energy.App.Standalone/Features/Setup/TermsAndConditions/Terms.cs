using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.TermsAndConditions;

[FeatureState(Name = nameof(TermsAndConditions))]
[PersistState, PriorityLoad]
public record TermsAndConditions
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
    public static TermsAndConditions Reduce(TermsAndConditions state, AcceptTermsAndConditionsAction action)
    {
        return state with { Accepted = action.Accepted };
    }

}
