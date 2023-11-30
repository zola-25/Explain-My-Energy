using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Household.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.App.Standalone.Features.Setup.UserState;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;

namespace Energy.App.Standalone.PageComponents.Storage;
public partial class StorageLock
{
    [Inject] ILogger<StorageLock> Logger { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    [Inject] IState<WeatherState> WeatherState { get; set; }

    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }

    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    [Inject] IDispatcher Dispatcher { get; set; }

    bool locking = false;
    bool inputError = false;
    bool lockingError = false;
    bool lockingSuccess = false;
    string lockingErrorMessage = String.Empty;

    private async ValueTask LockData()
    {
        locking = true;
        inputError = false;
        lockingError = false;
        lockingSuccess = false;
        lockingErrorMessage = String.Empty;


        string backupIHDMacId = HouseholdState.Value.IhdMacId;
        string backupHouseholdOutCode = HouseholdState.Value.OutCodeCharacters;
        string backupWeatherOutCode = WeatherState.Value.OutCode;
        string backupGasMeterMpxn = MeterSetupState.Value.GasMeter.Mpxn;
        string backupElectricityMeterMpxn = MeterSetupState.Value.ElectricityMeter.Mpxn;

        string input = model.Input1;

        try
        {
            if (String.IsNullOrWhiteSpace(input) || input.Length < 9 || input != model.Input2)
            {
                inputError = true;
                lockingErrorMessage = "Please enter a valid password";
                return;
            }

            if(!HouseholdState.Value.Saved || HouseholdState.Value.Invalid)
            {
                inputError = true;
                lockingErrorMessage = "Please save your household details before locking";
                return;
            }

            if(model.IhdMacId != HouseholdState.Value.IhdMacId)
            {
                inputError = true;
                lockingErrorMessage = "IHD MAC ID has been modified from the one saved";
                return;
            }

            Dispatcher.Dispatch(new SetSetupDataBeginLockingAction());

            string householdOutCode = HouseholdState.Value.OutCodeCharacters;

            string ihdMacId = HouseholdState.Value.IhdMacId;

            string newWeatherOutCodeValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", householdOutCode, input);
            string newIhdMacIdValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", ihdMacId, input);



            Dispatcher.Dispatch(new HouseholdStoreLockedData() {
                OutCodeCharacters = newWeatherOutCodeValue,
                OutCodeLocked = true,
                IhdMacId = newIhdMacIdValue,
                IhdMacIdLocked = true
            });

            string weatherOutCode = WeatherState.Value.OutCode;

            string newWeatherOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", weatherOutCode, input);

            Dispatcher.Dispatch(new StoreLockedWeatherOutcodeAction() { OutCode = newWeatherOutCode, OutCodeLocked = true });


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
                LockSuccess();
                return;
            }

            Dispatcher.Dispatch(new StoreLockedMeterDataAction {
                ElectricityMeterIdLocked = lockElectricityMeter,
                GasMeterIdLocked = lockGasMeter,
                ElectricityMeterMpan = newMpan,
                GasMeterMprn = newMprn
            });

            LockSuccess();
        }
        catch (Exception ex)
        {
            lockingError = true;
            Logger.LogError(ex, "Error locking data, attempting to rollback");

            // Rollback using dispatch to unlock actions

            try
            {
                Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());
                Dispatcher.Dispatch(new HouseholdRollbackLockDataAction() { OutCodeCharacters = backupHouseholdOutCode, IhdMacId = backupIHDMacId });
                Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = backupWeatherOutCode });
                Dispatcher.Dispatch(new MeterDataRollbackLockDataAction() { ElectricityMeterMpan = backupElectricityMeterMpxn, GasMeterMprn = backupGasMeterMpxn });
                Dispatcher.Dispatch(new SetSetupDataUnlockedAction());
                lockingErrorMessage = "Error locking data - all setup data remains unlocked. Please try again.";
            }
            catch (Exception rollbackEx)
            {
                Logger.LogError(rollbackEx, "Error rolling back data");
                lockingErrorMessage = "Critical Error locking data, unable to rollback - setup data may be partially encrypted.";
            }
        }
        finally
        {
            input = null;
            locking = false;
        }
    }


    void LockSuccess()
    {
        Dispatcher.Dispatch(new SetSetupDataLockedAction());
        if (model != null)
        {
            model.Input1 = null;
            model.Input2 = null;
        }
        lockingSuccess = true;
        locking = false;
    }
}
