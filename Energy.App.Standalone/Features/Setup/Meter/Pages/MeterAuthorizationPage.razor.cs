using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.Shared;
using Microsoft.AspNetCore.Components;

namespace Energy.App.Standalone.Features.Setup.Meter.Pages;
public partial class MeterAuthorizationPage
{
    [Inject]
    public IMeterAuthorizationCheck MeterAuthorizationCheck { get; set; }

    [Inject]
    ILogger<MeterAuthorizationPage> Logger { get; set; }

    [Parameter, EditorRequired]
    public string MeterTypeText { get; set; }

    [Parameter]
    public EventCallback OnSuccessfulCallback { get; set; }

    [Parameter]
    public bool FromWizard { get; set; }

    private string MpxnLabel => _meterType == MeterType.Gas ? "Gas MPRN" : "Electricity MPAN";

    private string MoveInDateString => HouseholdState.Value.MoveInDate!.Value.ToString("d");

    private bool AuthorizeSucceeded { get; set; }
    private bool AuthorizeFailed { get; set; }

    private string AuthorizeResponseMessage { get; set; }

    private bool ReadingsLoading { get; set; }

    private MeterType _meterType;


    protected override async Task OnParametersSetAsync()
    {
        _meterType = MeterTypeText.eStringToEnum<MeterType>();

        switch (_meterType)
        {
            case MeterType.Gas:
                {
                    SubscribeToAction<AuthorizeGasAction>((_) =>
                    {
                        LoadData(MeterType.Gas);
                    });
                    break;
                }

            case MeterType.Electricity:
                {
                    SubscribeToAction<AuthorizeElectricityAction>((_) =>
                    {
                        LoadData(MeterType.Electricity);
                    });
                    break;
                }

            default:
                break;
        }

        await CheckAuthorization();

    }

    private async void LoadData(MeterType meterType)
    {
        var taskCompletionSource = new TaskCompletionSource<(bool, string)>();
        ReadingsLoading = true;
        StateHasChanged();

        try
        {
            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(true, taskCompletionSource));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(true, taskCompletionSource));

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await taskCompletionSource.Task;

            ReadingsLoading = false;
            StateHasChanged();

            if (FromWizard)
            {
                await OnSuccessfulCallback.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing readings for Meter {MeterType}", meterType);
        }
        finally
        {
            ReadingsLoading = false;
        }


    }

    void ReturnToSetup()
    {
        NavigationManager.NavigateTo("/Setup");
    }

    private async Task CheckAuthorization()
    {
        var response = await MeterAuthorizationCheck.TestAccess(_meterType, HouseholdState.Value.IhdMacId);
        if (response.Success)
        {
            switch (_meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new AuthorizeGasAction());
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new AuthorizeElectricityAction());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AuthorizeSucceeded = true;
            AuthorizeFailed = false;
        }
        else
        {
            AuthorizeSucceeded = false;
            AuthorizeFailed = true;
            AuthorizeResponseMessage = response.FailureReason;
        }
    }
}
