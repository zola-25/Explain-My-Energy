using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.TermsAndConditions;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Components.Snackbar;

#pragma warning disable IDE0058

namespace Energy.App.Standalone.PageComponents;

public partial class DataValidatorBase : FluxorComponent
{
    [Inject]
    public IDispatcher Dispatcher { get; set; }
    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    public string UpdateStatus { get; private set; }


    public bool AppLoading;

    [Inject]
    IState<HouseholdState> HouseholdState { get; set; }
    [Inject]
    IState<TermsAndConditionsState> TermsAndConditionsState { get; set; }
    
    [Inject]
    NavigationManager  NavigationManager { get; set; }

    protected override void Dispose(bool disposing)
    {
        
        base.Dispose(disposing);
    }


    bool isFirstPageLoad = true;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        isFirstPageLoad = true;
    }

    protected override async Task OnInitializedAsync()
    {
        AppLoading = true;
        this.eLogToConsole(nameof(OnInitializedAsync));

        UpdateStatus = "Loading data...";
        await Task.Delay(1);
        StateHasChanged();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
        
        SubscribeToAction<EnsureWeatherLoadedAction>((action) => {
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add("Loading Weather Readings",  Severity.Info,
                    options=> { options.HideIcon=true; },key: "Loading Weather Readings" );
            }
        });

        SubscribeToAction<NotifyWeatherLoadingFinished>((action) => {
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.RemoveByKey("Loading Weather Readings");
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning,
                    options=> { options.HideIcon=true; } );
            }
        });

        SubscribeToAction<EnsureGasReadingsLoadedAction>((action) => {
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add("Loading Gas Readings", Severity.Info,
                    options=> { options.HideIcon=true; },key: "Loading Gas Readings" );
            }
        });

        SubscribeToAction<NotifyGasLoadingFinished>((action) => {
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.RemoveByKey("Loading Gas Readings");
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning,
                    options=> { options.HideIcon=true; });
            }
        });

        SubscribeToAction<EnsureElectricityReadingsLoadedAction>((action) => {
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add("Loading Electricity Readings", Severity.Info,
                    options=> { options.HideIcon=true; },key: "Loading Electricity Readings" );
            }
        });

        SubscribeToAction<NotifyElectricityLoadingFinished>((action) => {

            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.RemoveByKey("Loading Electricity Readings");
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning,
                    options => options.HideIcon = true);
            }
        });

        SubscribeToAction<NotifyHeatingSetupFinishedAction>((action) => {
           
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning, options => { options.HideIcon = true; });
            }
        });

        SubscribeToAction<NotifyHeatingForecastFinishedAction>((action) => {
            
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning, options => { options.HideIcon = true; });
            }
        });

        SubscribeToAction<NotifyElectricityForecastResult>((action) => {
            
            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning, options => { options.HideIcon = true; });
            }
        });
        SubscribeToAction<NotifyGasForecastResult>((action ) => {

            if (TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed && !isFirstPageLoad)
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning, options => { options.HideIcon = true; });
            }
        });


        var weatherReadingCompletion = new TaskCompletionSource<(bool, string)>();
        Dispatcher.Dispatch(new EnsureWeatherLoadedAction(false, weatherReadingCompletion));
        await weatherReadingCompletion.Task;


        UpdateStatus = "Loading energy readings...";
        await Task.Delay(1);
        StateHasChanged();

        var gasReadingsCompletion = new TaskCompletionSource<(bool, string)>();
        var electricityReadingsCompletion = new TaskCompletionSource<(bool, string)>();

        Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false, gasReadingsCompletion));

        Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(false, electricityReadingsCompletion));

        await Task.WhenAll(gasReadingsCompletion.Task, electricityReadingsCompletion.Task);



        await Task.Delay(1);
        StateHasChanged();
        UpdateStatus = "Ready";

        isFirstPageLoad = false;
        AppLoading = false;
    }
}