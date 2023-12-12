using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.Shared;
using Energy.App.Standalone.PageComponents;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;


namespace Energy.App.Standalone.Features.Setup.Meter.Pages;
public partial class MeterAuthorizationFormComponent
{
    [Parameter] public EventCallback<bool> OnSuccessfulCallback { get; set; }
    [Parameter, EditorRequired] public MeterType MeterType { get; set; }

    [Parameter, EditorRequired] public bool FreshNavigation { get; set; }


    [Inject] IState<GasReadingsState> GasReadingState { get; set; }
    [Inject] IState<ElectricityReadingsState> ElectricityReadingState { get; set; }


    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }
    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    [Inject] ILogger<MeterAuthorizationFormComponent> Logger { get; set; }

    bool ReadingsLoading => MeterType == MeterType.Gas ? GasReadingState.Value.Loading : ElectricityReadingState.Value.Loading;

    string MpxnLabel => MeterType == MeterType.Gas ? "Gas MPRN" : "Electricity MPAN";

    string MoveInDateString => HouseholdState.Value.MoveInDate!.Value.ToString("MMM yyyy");

    bool Authorizing => MeterSetupState.Value[MeterType].Authorizing;
    bool AuthorizeSucceeded => MeterSetupState.Value[MeterType].Authorized;
    bool AuthorizeFailed => MeterSetupState.Value[MeterType].AuthorizeFailed;

    string AuthorizeFailedMessage => MeterSetupState.Value[MeterType].AuthorizeFailedMessage;

    bool ParametersSet;
    bool readingsLoadedSuccess = false;
    bool readingsLoadingComplete = false;

    private bool _freshNavigation = true;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SubscribeToAction<NotifyGasLoadingFinished>(OnGasLoadingFinished);
        SubscribeToAction<NotifyElectricityLoadingFinished>(OnElectricityLoadingFinished);
    }

    private void OnElectricityLoadingFinished(NotifyElectricityLoadingFinished finished)
    {
        if(MeterType != MeterType.Electricity)
        {
            return;
        }
        readingsLoadedSuccess = finished.Success;
        if(!finished.Success)
        {
            Logger.LogError("Error loading Electricity Readings: {ErrorMessage}", finished.Message);
        }
        readingsLoadingComplete = true;
    }

    private void OnGasLoadingFinished(NotifyGasLoadingFinished finished)
    {
        if(MeterType != MeterType.Gas)
        {
            return;
        }
        readingsLoadedSuccess = finished.Success;
        if(!finished.Success)
        {
            Logger.LogError("Error loading Gas Readings: {ErrorMessage}", finished.Message);
        }
        readingsLoadingComplete = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            await base.OnParametersSetAsync();

            if(UserLockState.Value.LockingOrLocked)
            {
                ParametersSet = false;
                return;
            }

            if (FreshNavigation && _freshNavigation)
            {
                readingsLoadedSuccess = false;
                readingsLoadingComplete = false;
                if (AuthorizeFailed)
                {
                    Dispatcher.Dispatch(new ClearMeterAuthorizationFailure(MeterType));
                }

                _freshNavigation = false;
            }

            ParametersSet = true;
            if (AuthorizeSucceeded)
            {
                await OnSuccessfulCallback.InvokeAsync(true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while Authorizing {MeterType}", MeterType);
        }
    }

    private void OnCheckAuthorizationClicked(MouseEventArgs _)
    {
        try
        {
            readingsLoadedSuccess = false;
            readingsLoadingComplete = false;

            if (ParametersSet)
            {
                Dispatcher.Dispatch(new AuthorizeMeterAction(MeterType));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while authorizing Meter {MeterType}", MeterType);
        }
    }
}
