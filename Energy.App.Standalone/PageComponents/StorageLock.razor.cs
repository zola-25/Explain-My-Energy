using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Household.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.App.Standalone.Features.Setup.UserState;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Energy.App.Standalone.PageComponents;
public partial class StorageLock
{
    [Inject] ILogger<StorageLock> Logger { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    [Inject] IState<WeatherState> WeatherState { get; set; }

    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }

    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    // Navigation Lock while locking data
    [Inject] IDispatcher Dispatcher { get; set; }
    
    bool locking = false;


    private async void LockData()
    {
        locking = true;
        lockingError = false;

        string backupIHDMacId = HouseholdState.Value.IhdMacId.ToString();
        string backupOutCode = HouseholdState.Value.OutCodeCharacters;
        string backupWeatherOutCode = WeatherState.Value.OutCode;
        string backupGasMeterMpxn = MeterSetupState.Value.GasMeter.Mpxn;
        string backupElectricityMeterMpxn = MeterSetupState.Value.ElectricityMeter.Mpxn;

        try
        {
            Dispatcher.Dispatch(new SetSetupDataBeginLockingAction());

            string input = model.Input1;
            if (String.IsNullOrWhiteSpace(input) || input.Length < 9)
            {
                throw new ApplicationException("Error with lock password");
            }

            if (HouseholdState.Value.Saved && !HouseholdState.Value.Invalid && !String.IsNullOrWhiteSpace(HouseholdState.Value.OutCodeCharacters))
            {
                string householdOutCode = HouseholdState.Value.OutCodeCharacters.Trim().ToUpperInvariant();
                string weatherOutCode = WeatherState.Value.OutCode.Trim().ToUpperInvariant();

                if (householdOutCode != weatherOutCode)
                {
                    throw new ApplicationException("Error: Household and Weather OutCodes do not match");
                }


                string ihdMacId = HouseholdState.Value.IhdMacId;

                string newOutCodeValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", householdOutCode, input);
                string newIhdMacIdValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", ihdMacId, input);



                Dispatcher.Dispatch(new HouseholdStoreLockedDataWithWeatherAction() {
                    OutCodeCharacters = newOutCodeValue,
                    OutCodeLocked = true,
                    IhdMacId = newIhdMacIdValue,
                    IhdMacIdLocked = true
                });
            }

            var metersSetup = MeterSetupState.Value.MeterStates.Where(m => (m.InitialSetupValid || m.SetupValid) && !String.IsNullOrWhiteSpace(m.Mpxn));

            bool lockGasMeter = false;
            bool lockElectricityMeter = false;
            string newMprn = null;
            string newMpan = null;

            foreach (var meterState in metersSetup)
            {
                string mxpn = meterState.Mpxn;

                switch (meterState.MeterType)
                {
                    case Energy.Shared.MeterType.Gas:
                        newMprn = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", mxpn, input);
                        lockGasMeter = true;
                        break;
                    case Energy.Shared.MeterType.Electricity:
                        newMpan = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", mxpn, input);
                        lockElectricityMeter = true;
                        break;
                    default:
                        throw new ApplicationException("Error: Unknown Meter Type");
                }
            }
            if (!(lockGasMeter || lockElectricityMeter))
            {
                LockSuccess(ref input);
                return;
            }

            Dispatcher.Dispatch(new StoreLockedMeterDataAction {
                ElectricityMeterIdLocked = lockElectricityMeter,
                GasMeterIdLocked = lockGasMeter,
                ElectricityMeterMpan = newMpan,
                GasMeterMprn = newMprn
            });

            LockSuccess(ref input);
        }
        catch (Exception ex)
        {
            lockingError = true;
            Logger.LogError(ex, "Error locking data, attempting to rollback");

            // Rollback using dispatch to unlock actions

            try
            {
                Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());
                Dispatcher.Dispatch(new HouseholdStoreUnlockedDataAction() { OutCodeCharacters = backupOutCode, IhdMacId = backupIHDMacId });
                Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = backupWeatherOutCode });
                Dispatcher.Dispatch(new StoreUnlockedMeterDataAction() { ElectricityMeterMpan = backupElectricityMeterMpxn, GasMeterMprn = backupGasMeterMpxn });
                Dispatcher.Dispatch(new SetSetupDataUnlockedAction());
            }
            catch (Exception rollbackEx)
            {
                Logger.LogError(rollbackEx, "Error rolling back data");
            }
        }
        finally
        {
            locking = false;
        }
    }

    bool lockingError = false;

    void LockSuccess(ref string input)
    {
        input = null;
        model.Input1 = String.Empty;
        model.Input2 = String.Empty;
        Dispatcher.Dispatch(new SetSetupDataLockedAction());
        locking = false;
    }
}
