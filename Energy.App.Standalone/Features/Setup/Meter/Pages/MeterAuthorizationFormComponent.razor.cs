using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.Shared;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Energy.App.Standalone.Features.Setup.Meter.Pages;
public partial class MeterAuthorizationFormComponent
{
    [Parameter, EditorRequired] public EventCallback<(bool meterAuthSuccess, bool energyDataProcessSuccess, string message)> OnSuccessfulCallback { get; set; }
    [Parameter, EditorRequired] public MeterType MeterType { get; set; }
    [Parameter] public EventCallback<bool> ReadingsLoadingChanged { get; set; }
    [Parameter] public bool ReadingsLoading { get; set; }

    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }
    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    [Inject] ILogger<MeterAuthorizationFormComponent> Logger { get; set; }

    string MpxnLabel => MeterType == MeterType.Gas ? "Gas MPRN" : "Electricity MPAN";

    string MoveInDateString => HouseholdState.Value.MoveInDate!.Value.ToString("d");

    bool Authorizing => MeterSetupState.Value[MeterType].Authorizing;
    bool AuthorizeSucceeded => MeterSetupState.Value[MeterType].Authorized;
    bool AuthorizeFailed => MeterSetupState.Value[MeterType].AuthorizeFailed;

    string MeterProblemMessage => MeterSetupState.Value[MeterType].ProblemMessage;

    bool ParametersSet;


    bool _tryOnceToAuthorize;
    protected override void  OnParametersSet()
    {
        if(!_tryOnceToAuthorize && !Authorizing && !AuthorizeSucceeded) {
            Dispatcher.Dispatch(new AuthorizeMeterAction(MeterType));
        };

        ParametersSet = true;

    }

    private async Task LoadData(MeterType meterType)
    {
        await UpdateReadingsLoading(true);
        var taskCompletionSource = new TaskCompletionSource<(bool, string)>();
        (bool processingSuccess, string message) dataProcessingResults;
        try
        {
            StateHasChanged();

            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(true, taskCompletionSource));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(true, taskCompletionSource));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
            }

            dataProcessingResults = await taskCompletionSource.Task;

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing readings for Meter {MeterType}", meterType);
            taskCompletionSource.SetResult((false, $"Critical Error processing readings for Meter {meterType}"));
            dataProcessingResults = await taskCompletionSource.Task;
        }
        await UpdateReadingsLoading(false);
        await OnSuccessfulCallback.InvokeAsync((AuthorizeSucceeded, dataProcessingResults.processingSuccess, dataProcessingResults.message));
    }
    !

    bool _lastUpdatedReadingsLoading;
    bool _readingsLoadingFirstSet;
    private async Task UpdateReadingsLoading(bool readingsLoading)
    {
        if (!_readingsLoadingFirstSet)
        {
            _readingsLoadingFirstSet = true;
            _lastUpdatedReadingsLoading = readingsLoading;
            await ReadingsLoadingChanged.InvokeAsync(readingsLoading);
            return;
        }
        if(_lastUpdatedReadingsLoading == readingsLoading) return;

        _lastUpdatedReadingsLoading = readingsLoading;

        await ReadingsLoadingChanged.InvokeAsync(readingsLoading);
    }
}
