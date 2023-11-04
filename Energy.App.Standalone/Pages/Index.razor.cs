using Microsoft.AspNetCore.Components;
using Fluxor;
using Energy.App.Standalone.Features.Setup.TermsAndConditions;
using Energy.App.Standalone.Services;

namespace Energy.App.Standalone.Pages;

public partial class Index
{
    [Inject] IState<TermsAndConditionsState> TermsAndConditionsState { get; set; }

    [Inject] NavigationManager NavManager { get; set; }


    bool Ready;

    protected override void OnInitialized()
    {
        if (AppStatus.IsDemoMode && !AppStatus.HadAutoRedirect)
        {
            AppStatus.SetHadAutoRedirect();
            NavManager.NavigateTo("/HeatingMeter/Gas", replace: true);
        }
    }

    protected override void OnParametersSet()
    {
        Ready = false;
        base.OnParametersSet();

        var termsAccepted = TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed;
        bool defaultOpenWizard = !termsAccepted;
        OpenWizard = OpenWizard || defaultOpenWizard;

        Ready = true;
    }
}