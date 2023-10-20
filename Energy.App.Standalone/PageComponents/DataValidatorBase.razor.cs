using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
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


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    protected override async Task OnInitializedAsync()
    {
        AppLoading = true;
        this.eLogToConsole(nameof(OnInitializedAsync));

        UpdateStatus = "Loading data...";
        await Task.Delay(1);
        StateHasChanged();

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;

        SubscribeToAction<NotifyWeatherLoadingFinished>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
        });

        SubscribeToAction<NotifyGasLoadingFinished>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);

        });

        SubscribeToAction<NotifyElectricityLoadingFinished>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
        });

        SubscribeToAction<NotifyHeatingSetupFinishedAction>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);

        });

        SubscribeToAction<NotifyHeatingForecastFinishedAction>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
        });

        SubscribeToAction<NotifyElectricityForecastResult>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
        });
        SubscribeToAction<NotifyGasForecastResult>((action) =>
        {
            Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
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

        AppLoading = false;
    }
}