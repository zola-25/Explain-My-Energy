using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.UserState;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Energy.App.Standalone.PageComponents.Storage;
public partial class StorageUnlock
{
    [Inject] ILogger<StorageUnlock> Logger { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    [Inject] IState<WeatherState> WeatherState { get; set; }

    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }

    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    [Inject] IDispatcher Dispatcher { get; set; }

    bool unlocking = false;
    bool unlockError = false;
    bool unlockComplete = false;
    bool inputError = false;
    string unlockErrorMessage = String.Empty;

    bool householdOutCodeUnlockedSuccess = false;
    bool householdIhdMacIdUnlockedSuccess = false;
    bool weatherOutCodeUnlockedSuccess = false;
    bool gasMeterMpxnUnlockedSuccess = false;
    bool electricityMeterMpxnUnlockedSuccess = false;
    Severity householdOutCodeUnlockSeverity => householdOutCodeUnlockedSuccess ? Severity.Info : Severity.Warning;
    Severity householdIhdMacIdUnlockSeverity => householdIhdMacIdUnlockedSuccess ? Severity.Info : Severity.Warning;
    Severity weatherOutCodeUnlockSeverity => weatherOutCodeUnlockedSuccess ? Severity.Info : Severity.Warning;
    Severity gasMeterMpxnUnlockSeverity => gasMeterMpxnUnlockedSuccess ? Severity.Info : Severity.Warning;
    Severity electricityMeterMpxnUnlockSeverity => electricityMeterMpxnUnlockedSuccess ? Severity.Info : Severity.Warning;

    string householdOutCodeUnlockResult = "";
    string householdIhdMacIdUnlockResult = "";
    string weatherOutCodeUnlockResult = "";
    string gasMeterMpxnUnlockResult = "";
    string electricityMeterMpxnUnlockResult = "";


    private async void UnlockData()
    {
        unlocking = true;
        unlockComplete = false;
        unlockError = false;

        householdOutCodeUnlockedSuccess = false;
        householdIhdMacIdUnlockedSuccess = false;
        weatherOutCodeUnlockedSuccess = false;
        gasMeterMpxnUnlockedSuccess = false;
        electricityMeterMpxnUnlockedSuccess = false;


        string backupIHDMacId = HouseholdState.Value.IhdMacId;
        string backupHouseholdOutCode = HouseholdState.Value.OutCodeCharacters;
        string backupWeatherOutCode = WeatherState.Value.OutCode;
        string backupGasMeterMpxn = MeterSetupState.Value.GasMeter.Mpxn;
        string backupElectricityMeterMpxn = MeterSetupState.Value.ElectricityMeter.Mpxn;
        string input = model.Input;
        string ihdMacIdInput = model.IhdMacId;

        try
        {

            if (String.IsNullOrEmpty(input))
            {
                inputError = true;
                unlockErrorMessage = "Please enter a password";
                return;
            }

            if (String.IsNullOrEmpty(ihdMacIdInput))
            {
                inputError = true;
                unlockErrorMessage = "Please enter your IHD MAC ID";
                return;
            }

            bool verified = await Verify(ihdMacIdInput, input);
            if (!verified)
            {
                return;
            }


            Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());

            await UnlockHouseholdOutcode(backupHouseholdOutCode, input);

            await UnlockHouseholdIhdMacId(backupIHDMacId, input);
            await UnlockWeatherOutCode(backupWeatherOutCode, input);

            await UnlockGasMeterMpxn(backupGasMeterMpxn, input);
            await UnlockElectricityMeterMpxn(backupElectricityMeterMpxn, input);

            Dispatcher.Dispatch(new SetSetupDataUnlockedAction());

            UnlockFinished();
        }
        catch (Exception ex)
        {   // TODO: unlock if verified, keep locked if not
            unlockError = true;
            Logger.LogError(ex, "Error unlocking data");
            Dispatcher.Dispatch(new SetSetupDataLockedAction());
        }
        finally
        {
            unlocking = false;
        }
    }

    private async Task<bool> Verify(string ihdMacIdInput, string input)
    {
        try
        {
            string lockedIhdMacId = HouseholdState.Value.IhdMacId;
            bool verified = await JSRuntime.InvokeAsync<bool>("StorageHelper.verify", lockedIhdMacId, input, ihdMacIdInput);
            if (!verified)
            {
                inputError = true;
                unlockErrorMessage = "Invalid password or IHD MAC ID";
                return false;
            }
            else
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error verifying password");
            unlockError = true;
            unlockErrorMessage = "Error verifying password";
            return false;
        }
    }

    void UnlockFinished()
    {
        unlockError = !householdIhdMacIdUnlockedSuccess || !householdOutCodeUnlockedSuccess || !weatherOutCodeUnlockedSuccess || !gasMeterMpxnUnlockedSuccess || !electricityMeterMpxnUnlockedSuccess;

        if (model != null)
        {
            model.Input = null;
        }
        unlocking = false;
    }

    private async Task UnlockGasMeterMpxn(string lockedGasMeterMpxn, string input)
    {
        try
        {
            if (MeterSetupState.Value.GasMeter.MpxnLocked)
            {
                string gasMeterMpxn;
                if (!String.IsNullOrEmpty(lockedGasMeterMpxn))
                {
                    gasMeterMpxn = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedGasMeterMpxn, input);
                    if (gasMeterMpxn.eIsValidMprn())
                    {
                        gasMeterMpxnUnlockedSuccess = true;
                        gasMeterMpxnUnlockResult = "Gas meter mpxn unlocked successfully";
                    }
                    else
                    {
                        gasMeterMpxnUnlockedSuccess = false;
                        gasMeterMpxnUnlockResult = $"Unlocked gas meter MPRN is invalid - {gasMeterMpxn}";
                    }
                }
                else
                {
                    gasMeterMpxnUnlockedSuccess = true;
                    gasMeterMpxn = lockedGasMeterMpxn;
                    gasMeterMpxnUnlockResult = "No gas meter MPRN to unlock";
                }

                Dispatcher.Dispatch(new StoreUnlockedGasMprnAction() { GasMeterMprn = gasMeterMpxn });
            }
            else
            {
                gasMeterMpxnUnlockResult = "Gas meter MPRN already unlocked";
                gasMeterMpxnUnlockedSuccess = true;
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking gas meter MPRN");
            gasMeterMpxnUnlockedSuccess = false;
            Dispatcher.Dispatch(new StoreUnlockedGasMprnAction() { GasMeterMprn = lockedGasMeterMpxn });
            gasMeterMpxnUnlockResult = "Error unlocking gas meter MPRN - enabling access anyway";
        }

    }

    private async Task UnlockElectricityMeterMpxn(string lockedElectricityMeterMpxn, string input)
    {
        try
        {
            if (MeterSetupState.Value.ElectricityMeter.MpxnLocked)
            {
                string electricityMeterMpxn;
                if (!String.IsNullOrEmpty(lockedElectricityMeterMpxn))
                {
                    electricityMeterMpxn = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedElectricityMeterMpxn, input);
                    if (electricityMeterMpxn.eIsValidMpan())
                    {
                        electricityMeterMpxnUnlockedSuccess = true;
                        electricityMeterMpxnUnlockResult = "Electricity meter mpxn unlocked successfully";
                    }
                    else
                    {
                        electricityMeterMpxnUnlockedSuccess = false;
                        electricityMeterMpxnUnlockResult = $"Unlocked electricity meter MPAN is invalid - {electricityMeterMpxn}";
                    }
                }
                else
                {
                    electricityMeterMpxnUnlockedSuccess = true;
                    electricityMeterMpxn = lockedElectricityMeterMpxn;
                    electricityMeterMpxnUnlockResult = "No electricity meter MPAN to unlock";
                }

                Dispatcher.Dispatch(new StoreUnlockedElectricityMprnAction() { ElectricityMeterMprn = electricityMeterMpxn });
            }
            else
            {
                electricityMeterMpxnUnlockResult = "Electricity meter mpxn already unlocked";
                electricityMeterMpxnUnlockedSuccess = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking electricity meter mpxn");
            electricityMeterMpxnUnlockedSuccess = false;
            Dispatcher.Dispatch(new StoreUnlockedElectricityMprnAction() { ElectricityMeterMprn = lockedElectricityMeterMpxn });
            electricityMeterMpxnUnlockResult = "Error unlocking electricity meter mpxn - enabling access anyway";
        }

    }

    private async Task UnlockWeatherOutCode(string lockedWeatherOutCode, string input)
    {
        try
        {
            if (WeatherState.Value.OutCodeLocked)
            {
                string weatherOutCode;
                if (!String.IsNullOrEmpty(lockedWeatherOutCode))
                {
                    weatherOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedWeatherOutCode, input);
                    if(weatherOutCode.eIsValidOutcode())
                    {
                        weatherOutCodeUnlockedSuccess = true;
                        weatherOutCodeUnlockResult = "Weather postal area unlocked successfully";
                    }
                    else
                    {
                        weatherOutCodeUnlockedSuccess = false;
                        weatherOutCodeUnlockResult = $"Unlocked weather postal area is invalid - {weatherOutCode}";
                    }
                }
                else
                {
                    weatherOutCodeUnlockedSuccess = true;
                    weatherOutCode = lockedWeatherOutCode;
                    weatherOutCodeUnlockResult = "No weather postal area to unlock";
                }

                Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = weatherOutCode });
            }
            else
            {
                weatherOutCodeUnlockResult = "Weather postal area already unlocked";
                weatherOutCodeUnlockedSuccess = true;

            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking weather postal area");
            weatherOutCodeUnlockedSuccess = false;
            Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = lockedWeatherOutCode });
            weatherOutCodeUnlockResult = "Error unlocking weather postal area - enabling access anyway";
        }

    }

    private async Task UnlockHouseholdIhdMacId(string lockedIHDMacId, string input)
    {
        try
        {
            if (HouseholdState.Value.IhdMacIdLocked)
            {
                string ihdMacId;
                if (!String.IsNullOrEmpty(lockedIHDMacId))
                {
                    ihdMacId = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedIHDMacId, input);
                    if(ihdMacId.Length != 16)
                    {
                        householdIhdMacIdUnlockedSuccess = false;
                        householdIhdMacIdUnlockResult = $"Unlocked IHD MAC ID is invalid - {ihdMacId}";
                    }
                    else
                    {
                        householdIhdMacIdUnlockedSuccess = true;
                        householdIhdMacIdUnlockResult = "Household IHD Mac ID unlocked successfully";
                    }

                }
                else
                {
                    householdIhdMacIdUnlockedSuccess = true;
                    ihdMacId = lockedIHDMacId;
                    householdIhdMacIdUnlockResult = "No household IHD Mac ID to unlock";
                }

                Dispatcher.Dispatch(new HouseholdStoreUnlockedIhdMacIdAction() { IHDMacIDCharacters = ihdMacId });
            }
            else
            {
                householdIhdMacIdUnlockResult = "Household IHD Mac ID already unlocked";
                householdIhdMacIdUnlockedSuccess = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking household IHD Mac ID");
            householdIhdMacIdUnlockedSuccess = false;
            Dispatcher.Dispatch(new HouseholdStoreUnlockedIhdMacIdAction() { IHDMacIDCharacters = lockedIHDMacId });
            householdIhdMacIdUnlockResult = "Error unlocking household IHD Mac ID - enabling access anyway";
        }
    }

    private async Task UnlockHouseholdOutcode(string lockedHouseholdOutcode, string input)
    {
        try
        {
            if (HouseholdState.Value.OutCodeLocked)
            {
                string householdOutCode;
                if (!String.IsNullOrEmpty(lockedHouseholdOutcode))
                {
                    householdOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedHouseholdOutcode, input);
                    if(householdOutCode.eIsValidOutcode())
                    {
                        householdOutCodeUnlockedSuccess = true;
                        householdOutCodeUnlockResult = "Household postal area unlocked successfully";
                    }
                    else
                    {
                        householdOutCodeUnlockedSuccess = false;
                        householdOutCodeUnlockResult = $"Unlocked household postal area is invalid - {householdOutCode}";
                    }
                }
                else
                {
                    householdOutCodeUnlockedSuccess = true;
                    householdOutCode = lockedHouseholdOutcode;
                    householdOutCodeUnlockResult = "No household postal area to unlock";
                }

                Dispatcher.Dispatch(new HouseholdStoreUnlockedOutCodeAction() { OutCodeCharacters = householdOutCode });
            }
            else
            {
                householdOutCodeUnlockResult = "Household postal area already unlocked";
                householdOutCodeUnlockedSuccess = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking household outcode");
            householdOutCodeUnlockedSuccess = false;
            Dispatcher.Dispatch(new HouseholdStoreUnlockedOutCodeAction() { OutCodeCharacters = lockedHouseholdOutcode });
            householdOutCodeUnlockResult = "Error unlocking household outcode - enabling access anyway";
        }
    }


}
