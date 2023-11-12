using Energy.App.Standalone.Features.Setup.TermsAndConditions;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Energy.App.Standalone.Pages;

public partial class Index
{
    [Inject] IState<TermsAndConditionsState> TermsAndConditionsState { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        bool termsAccepted = TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed;
        bool defaultOpenWizard = !termsAccepted;
        OpenWizard = OpenWizard || defaultOpenWizard;
    }

}