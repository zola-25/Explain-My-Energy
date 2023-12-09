using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.UserState;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.PageComponents.Storage.StorageModels;
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
    bool unlockSuccess = false;
    bool inputError = false;
    string unlockErrorMessage = String.Empty;

    bool verified = false;


    private Dictionary<LockableDataEnum, UnlockResult> unlockResults = new Dictionary<LockableDataEnum, UnlockResult>() {
        { LockableDataEnum.HouseholdOutCode, new UnlockResult() { Success = false, Message = "Not attempted" } },
        { LockableDataEnum.HouseholdIhdMacId, new UnlockResult() { Success = false, Message = "Not attempted" } },
        { LockableDataEnum.WeatherOutCode, new UnlockResult() { Success = false, Message = "Not attempted" } },
        { LockableDataEnum.GasMeterMprn, new UnlockResult() { Success = false, Message = "Not attempted" } },
        { LockableDataEnum.ElectricityMeterMpan, new UnlockResult() { Success = false, Message = "Not attempted" } }
    };

    private void ResetUnlockResults()
    {
        unlockResults = new Dictionary<LockableDataEnum, UnlockResult>() {
            { LockableDataEnum.HouseholdOutCode, new UnlockResult() { Success = false, Message = "Not attempted" } },
            { LockableDataEnum.HouseholdIhdMacId, new UnlockResult() { Success = false, Message = "Not attempted" } },
            { LockableDataEnum.WeatherOutCode, new UnlockResult() { Success = false, Message = "Not attempted" } },
            { LockableDataEnum.GasMeterMprn, new UnlockResult() { Success = false, Message = "Not attempted" } },
            { LockableDataEnum.ElectricityMeterMpan, new UnlockResult() { Success = false, Message = "Not attempted" } }
        };
    }

    private async Task UnlockData()
    {
        unlocking = true;
        ResetUnlockResults();
        inputError = false;
        unlockSuccess = false;
        unlockError = false;
        unlockErrorMessage = String.Empty;
        verified = false;


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
            await Task.Delay(TimeSpan.FromSeconds(1));


            verified = await VerifyAndUnlockIhdMacId(ihdMacIdInput, input);
            if (!verified)
            {
                return;
            }

            await UnlockHouseholdOutcode(backupHouseholdOutCode, input);

            await UnlockWeatherOutCode(backupWeatherOutCode, input);

            await UnlockGasMeterMpxn(backupGasMeterMpxn, input);
            await UnlockElectricityMeterMpxn(backupElectricityMeterMpxn, input);

            Dispatcher.Dispatch(new SetSetupDataUnlockedAction());

            unlockError = unlockResults.Any(x => !x.Value.Success);

            if (!unlockError && model != null)
            {
                model.Input = null;
                unlockSuccess = true;
            }
            unlocking = false;
        }
        catch (Exception ex)
        {
            unlockError = true;
            Logger.LogError(ex, "Unable to complete unlocking all data");

            if (verified)
            {
                Dispatcher.Dispatch(new SetSetupDataUnlockedAction());
                unlockErrorMessage = "Unable to complete unlocking all data";
            }
            else
            {
                Dispatcher.Dispatch(new SetSetupDataLockedAction());
            }
        }
        finally
        {
            unlocking = false;
        }
    }

    private async Task<bool> VerifyAndUnlockIhdMacId(string ihdMacIdInput, string input)
    {
        try
        {
            string lockedIhdMacId = HouseholdState.Value.IhdMacId;
            if (String.IsNullOrEmpty(lockedIhdMacId))
            {
                verified = false;
                inputError = true;
                unlockErrorMessage = "No encrypted household IHD Mac ID to unlock";
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Success = false;
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Message = "No encrypted household IHD Mac ID to unlock";
                return false;
            }
            string result = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedIhdMacId, input);
            if (result != ihdMacIdInput)
            {
                verified = false;
                inputError = true;
                unlockErrorMessage = "Invalid password or IHD MAC ID";
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Success = false;
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Message = "Invalid password or IHD MAC ID";
                return false;
            } 
            else if (result.Length != 16)
            {
                verified = false;
                inputError = true;
                unlockErrorMessage = "Invalid IHD MAC ID";
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Success = false;
                unlockResults[LockableDataEnum.HouseholdIhdMacId].Message = $"Unlocked IHD MAC ID is invalid - {result}";
                return false;
            }

            verified = true;

            Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());

            unlockResults[LockableDataEnum.HouseholdIhdMacId].Success = true;
            unlockResults[LockableDataEnum.HouseholdIhdMacId].Message = "Household IHD Mac ID unlocked successfully";

            Dispatcher.Dispatch(new HouseholdStoreUnlockedIhdMacIdAction() { IHDMacIDCharacters = result });

            return true;
        }
        catch (JSException)
        {
            verified = false;
            Logger.LogError("Unable to verify password");

            inputError = true;
            unlockErrorMessage = "Unable to verify password";
            return false;
        }
        catch (Exception ex)
        {
            verified = false;
            Logger.LogError(ex, "Error verifying password");

            inputError = true;
            unlockErrorMessage = "Error verifying password";
            return false;
        }
    }

    private async Task UnlockGasMeterMpxn(string lockedGasMeterMpxn, string input)
    {
        try
        {
            if (String.IsNullOrEmpty(lockedGasMeterMpxn))
            {
                unlockResults[LockableDataEnum.GasMeterMprn].Success = true;
                unlockResults[LockableDataEnum.GasMeterMprn].Message = "No gas meter MPRN to unlock";
            }
            else
            {
                string gasMeterMpxn = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedGasMeterMpxn, input);
                if (gasMeterMpxn.eIsValidMprn())
                {
                    unlockResults[LockableDataEnum.GasMeterMprn].Success = true;
                    unlockResults[LockableDataEnum.GasMeterMprn].Message = "Gas meter mpxn unlocked successfully";
                    Dispatcher.Dispatch(new StoreUnlockedGasMprnAction() { GasMeterMprn = gasMeterMpxn });
                }
                else
                {
                    unlockResults[LockableDataEnum.GasMeterMprn].Success = false;
                    unlockResults[LockableDataEnum.GasMeterMprn].Message = $"Unlocked gas meter MPRN is invalid - {gasMeterMpxn}";
                }
            }
        }
        catch (JSException)
        {
            Logger.LogError("Unable to decrypt gas meter MPRN");
            unlockResults[LockableDataEnum.GasMeterMprn].Success = false;
            unlockResults[LockableDataEnum.GasMeterMprn].Message = "Unable to decrypt gas meter MPRN";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking gas meter MPRN");

            unlockResults[LockableDataEnum.GasMeterMprn].Success = false;
            unlockResults[LockableDataEnum.GasMeterMprn].Message = "Error unlocking gas meter MPRN";
        }
    }

    private async Task UnlockElectricityMeterMpxn(string lockedElectricityMeterMpxn, string input)
    {
        try
        {
            if (String.IsNullOrEmpty(lockedElectricityMeterMpxn))
            {
                unlockResults[LockableDataEnum.ElectricityMeterMpan].Success = true;
                unlockResults[LockableDataEnum.ElectricityMeterMpan].Message = "No electricity meter MPAN to unlock";
            }
            else
            {
                string electricityMeterMpxn = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedElectricityMeterMpxn, input);
                if (electricityMeterMpxn.eIsValidMpan())
                {
                    unlockResults[LockableDataEnum.ElectricityMeterMpan].Success = true;
                    unlockResults[LockableDataEnum.ElectricityMeterMpan].Message = "Electricity meter MPAN unlocked successfully";
                    Dispatcher.Dispatch(new StoreUnlockedElectricityMprnAction() { ElectricityMeterMprn = electricityMeterMpxn });
                }
                else
                {
                    unlockResults[LockableDataEnum.ElectricityMeterMpan].Success = false;
                    unlockResults[LockableDataEnum.ElectricityMeterMpan].Message = $"Unlocked electricity meter MPAN is invalid - {electricityMeterMpxn}";
                }
            }
        }
        catch (JSException)
        {
            Logger.LogError("Unable to decrypt electricity meter mpxn");
            unlockResults[LockableDataEnum.ElectricityMeterMpan].Success = false;
            unlockResults[LockableDataEnum.ElectricityMeterMpan].Message = "Unable to decrypt electricity meter mpxn";
        }

        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking electricity meter mpxn");
            unlockResults[LockableDataEnum.ElectricityMeterMpan].Success = false;
            unlockResults[LockableDataEnum.ElectricityMeterMpan].Message = "Error unlocking electricity meter mpxn";
        }
    }

    private async Task UnlockWeatherOutCode(string lockedWeatherOutCode, string input)
    {
        try
        {
            if (String.IsNullOrEmpty(lockedWeatherOutCode))
            {
                unlockResults[LockableDataEnum.WeatherOutCode].Success = true;
                unlockResults[LockableDataEnum.WeatherOutCode].Message = "No weather postal area to unlock";
            }
            else
            {
                string weatherOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedWeatherOutCode, input);
                if (weatherOutCode.eIsValidOutcode())
                {
                    unlockResults[LockableDataEnum.WeatherOutCode].Success = true;
                    unlockResults[LockableDataEnum.WeatherOutCode].Message = "Weather postal area unlocked successfully";
                    Dispatcher.Dispatch(new StoreUnlockedWeatherOutcodeAction() { OutCode = weatherOutCode });

                }
                else
                {
                    unlockResults[LockableDataEnum.WeatherOutCode].Success = false;
                    unlockResults[LockableDataEnum.WeatherOutCode].Message = $"Unlocked weather postal area is invalid - {weatherOutCode}";
                }
            }
        }
        catch (JSException)
        {
            Logger.LogError("Unable to decrypt weather postal area");
            unlockResults[LockableDataEnum.WeatherOutCode].Success = false;
            unlockResults[LockableDataEnum.WeatherOutCode].Message = "Unable to decrypt weather postal area";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking weather postal area");
            unlockResults[LockableDataEnum.WeatherOutCode].Success = false;
            unlockResults[LockableDataEnum.WeatherOutCode].Message = "Error unlocking weather postal area";
        }
    }


    private async Task UnlockHouseholdOutcode(string lockedHouseholdOutcode, string input)
    {
        try
        {
            if (String.IsNullOrEmpty(lockedHouseholdOutcode))
            {
                unlockResults[LockableDataEnum.HouseholdOutCode].Success = false;
                unlockResults[LockableDataEnum.HouseholdOutCode].Message = "No household postal area to unlock";

                return;
            }
            string householdOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", lockedHouseholdOutcode, input);
            if (householdOutCode.eIsValidOutcode())
            {
                unlockResults[LockableDataEnum.HouseholdOutCode].Success = true;
                unlockResults[LockableDataEnum.HouseholdOutCode].Message = "Household postal area unlocked successfully";
                Dispatcher.Dispatch(new HouseholdStoreUnlockedOutCodeAction() { OutCodeCharacters = householdOutCode });
            }
            else
            {
                unlockResults[LockableDataEnum.HouseholdOutCode].Success = false;
                unlockResults[LockableDataEnum.HouseholdOutCode].Message = $"Unlocked household postal area is invalid - {householdOutCode}";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error unlocking household postal area");

            unlockResults[LockableDataEnum.HouseholdOutCode].Success = false;
            unlockResults[LockableDataEnum.HouseholdOutCode].Message = "Error unlocking household postal area";
        }
    }


}
