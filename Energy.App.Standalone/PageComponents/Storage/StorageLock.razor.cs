using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Household.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.App.Standalone.Features.Setup.UserState;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.PageComponents.Storage.Exceptions;
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


        string originalIHDMacId = HouseholdState.Value.IhdMacId;
        string originalHouseholdOutCode = HouseholdState.Value.OutCodeCharacters;
        string originalWeatherOutCode = WeatherState.Value.OutCode;
        string originalGasMeterMpxn = MeterSetupState.Value.GasMeter.Mpxn;
        string originalElectricityMeterMpxn = MeterSetupState.Value.ElectricityMeter.Mpxn;

        string input = model.Input1;

        try
        {
            if (String.IsNullOrWhiteSpace(input) || input.Length < 9 || input != model.Input2)
            {
                inputError = true;
                lockingErrorMessage = "Please enter a valid password";
                return;
            }

            if (!HouseholdState.Value.Saved || HouseholdState.Value.Invalid)
            {
                inputError = true;
                lockingErrorMessage = "Please save your household details before locking";
                return;
            }

            if (model.IhdMacId != HouseholdState.Value.IhdMacId)
            {
                inputError = true;
                lockingErrorMessage = "IHD MAC ID has been modified from the one saved";
                return;
            }

            if (String.IsNullOrEmpty(originalWeatherOutCode))
            {
                inputError = true;
                lockingErrorMessage = "Weather postal area is empty";
                return;
            }


            await Task.Delay(TimeSpan.FromSeconds(1));


            Dispatcher.Dispatch(new SetSetupDataBeginLockingAction());



            string newIhdMacIdValue = null;
            try
            {
                newIhdMacIdValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", originalIHDMacId, input);
                if (String.IsNullOrEmpty(newIhdMacIdValue))
                {
                    lockingError = true;
                    Logger.LogError("Encrypted IHD MAC ID is empty");
                    throw new EncryptedDataEmptyException("Encrypted IHD MAC ID is empty");
                }
            }
            catch (JSException)
            {
                lockingError = true;
                Logger.LogError("Unable to encrypt IHD MAC ID");
                throw new JSEncryptionException("Unable to encrypt IHD MAC ID");
            }


            string newWeatherOutCodeValue = null;
            try
            {
                newWeatherOutCodeValue = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", originalWeatherOutCode, input);

                if (String.IsNullOrEmpty(newWeatherOutCodeValue))
                {
                    throw new EncryptedDataEmptyException("Encrypted weather postal area is empty");
                }
            }
            catch (JSException)
            {
                lockingError = true;
                Logger.LogError("Unable to encrypt weather postal area");
                throw new JSEncryptionException("Unable to encrypt weather postal area");
            }

            Dispatcher.Dispatch(new HouseholdStoreLockedData() {
                OutCodeCharacters = newWeatherOutCodeValue,
                IhdMacId = newIhdMacIdValue,
            });


            string newWeatherOutCode = null;
            try
            {
                newWeatherOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", originalWeatherOutCode, input);
                if (String.IsNullOrEmpty(newWeatherOutCode))
                {
                    lockingError = true;
                    Logger.LogError("Encrypted weather postal area is empty");
                    throw new EncryptedDataEmptyException("Encrypted weather postal area is empty");
                }
            }
            catch (JSException)
            {
                lockingError = true;
                Logger.LogError("Unable to encrypt weather postal area");
                throw new JSEncryptionException("Unable to encrypt weather postal area");
            }
            Dispatcher.Dispatch(new StoreLockedWeatherOutcodeAction() { OutCode = newWeatherOutCode });


            var metersSetup = MeterSetupState.Value.MeterStates.Where(m => m.InitialSetupValid && !String.IsNullOrEmpty(m.Mpxn));

            string newMprn = originalGasMeterMpxn;
            string newMpan = originalElectricityMeterMpxn;

            foreach (var meterState in metersSetup)
            {
                string mxpn = meterState.Mpxn;

                switch (meterState.MeterType)
                {
                    case Energy.Shared.MeterType.Gas:
                        string encryptedMprn = null;
                        try
                        {
                            encryptedMprn = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", mxpn, input);
                            if (String.IsNullOrEmpty(encryptedMprn))
                            {
                                lockingError = true;
                                Logger.LogError("Encrypted gas meter MPRN is empty");
                                throw new EncryptedDataEmptyException("Encrypted gas meter MPRN is empty");
                            }
                            newMprn = encryptedMprn;
                        }
                        catch (JSException)
                        {
                            lockingError = true;
                            Logger.LogError("Unable to encrypt gas meter MPRN");
                            throw new JSEncryptionException("Unable to encrypt gas meter MPRN");
                        }
                        break;
                    case Energy.Shared.MeterType.Electricity:
                        string encryptedMpan = null;
                        try
                        {
                            encryptedMpan = await JSRuntime.InvokeAsync<string>("StorageHelper.encrypt", mxpn, input);
                            if (String.IsNullOrEmpty(encryptedMpan))
                            {
                                lockingError = true;
                                Logger.LogError("Encrypted electricity meter MPAN is empty");
                                throw new EncryptedDataEmptyException("Encrypted electricity meter MPAN is empty");
                            }
                            newMpan = encryptedMpan;
                        }
                        catch (JSException)
                        {
                            lockingError = true;
                            Logger.LogError("Unable to encrypt electricity meter MPAN");
                            throw new JSEncryptionException("Unable to encrypt electricity meter MPAN");
                        }
                        break;
                    default:
                        throw new ApplicationException("Error: Unknown Meter Type");
                }
            }

            Dispatcher.Dispatch(new StoreLockedMeterDataAction {
                ElectricityMeterMpan = newMpan,
                GasMeterMprn = newMprn
            });

            Dispatcher.Dispatch(new SetSetupDataLockedAction());
            if (model != null)
            {
                model.Input1 = null;
                model.Input2 = null;
            }
            lockingSuccess = true;
            locking = false;
        }
        catch (Exception ex)
        {
            lockingError = true;
            Logger.LogError(ex, "Error locking data, attempting to rollback");

            // Rollback using dispatch to unlock actions

            try
            {
                Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());
                Dispatcher.Dispatch(new HouseholdRollbackLockDataAction() { OutCodeCharacters = originalHouseholdOutCode, IhdMacId = originalIHDMacId });
                Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = originalWeatherOutCode });
                Dispatcher.Dispatch(new MeterDataRollbackLockDataAction() { ElectricityMeterMpan = originalElectricityMeterMpxn, GasMeterMprn = originalGasMeterMpxn });
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
            locking = false;
        }
    }
}
