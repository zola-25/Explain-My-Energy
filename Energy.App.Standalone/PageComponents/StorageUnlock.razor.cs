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
public partial class StorageUnlock
{
    [Inject] ILogger<StorageUnlock> Logger { get; set; }

    [Inject] IJSRuntime JSRuntime { get; set; }

    [Inject] IState<WeatherState> WeatherState { get; set; }

    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }

    [Inject] IState<HouseholdState> HouseholdState { get; set; }

    // Navigation Lock while locking data
    [Inject] IDispatcher Dispatcher { get; set; }

    bool unlocking = false;


    private async void UnlockData()
    {
        unlocking = true;
        unlockingError = false;

        string backupIHDMacId = HouseholdState.Value.IhdMacId;
        string backupOutCode = HouseholdState.Value.OutCodeCharacters;
        string backupWeatherOutCode = WeatherState.Value.OutCode;
        string backupGasMeterMpxn = MeterSetupState.Value.GasMeter.Mpxn;
        string backupElectricityMeterMpxn = MeterSetupState.Value.ElectricityMeter.Mpxn;

        try
        {
            Dispatcher.Dispatch(new SetSetupDataBeginUnlockingAction());

            if (String.IsNullOrEmpty(model.Input))
            {
                throw new ApplicationException("Error with unlock password");
            }
            string input = model.Input;


            string householdOutCode = null;

            if (HouseholdState.Value.OutCodeLocked)
            {
                if (!String.IsNullOrEmpty(HouseholdState.Value.OutCodeCharacters))
                {
                    householdOutCode = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", HouseholdState.Value.OutCodeCharacters, model.Input);
                }
            }

            string ihdMacId = null;
            if(HouseholdState.Value.IhdMacIdLocked)
            {
                if (!String.IsNullOrEmpty(HouseholdState.Value.IhdMacId))
                {
                    ihdMacId = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", HouseholdState.Value.IhdMacId, model.Input);
                }
            }

            Dispatcher.Dispatch(new HouseholdStoreUnlockedDataAction() {
                OutCodeCharacters = householdOutCode,
                IhdMacId = ihdMacId,
            });

            var metersSetup = MeterSetupState.Value.MeterStates.Where(m => m.MpxnLocked);

            string newMprn = null;
            string newMpan = null;
            bool unlockGasMeter = false;
            bool unlockElectricityMeter = false;

            foreach (var meterState in metersSetup)
            {

                switch (meterState.MeterType)
                {
                    case Energy.Shared.MeterType.Gas:
                        unlockGasMeter = true;
                        if(!String.IsNullOrEmpty(meterState.Mpxn))
                        {
                            newMprn = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", meterState.Mpxn, model.Input);
                        }
                        break;
                    case Energy.Shared.MeterType.Electricity:
                        unlockElectricityMeter = true;
                        if (!String.IsNullOrEmpty(meterState.Mpxn))
                        {
                            newMpan = await JSRuntime.InvokeAsync<string>("StorageHelper.decrypt", meterState.Mpxn, model.Input);
                        }
                        break;
                    default:
                        throw new ApplicationException("Error: Unknown Meter Type");
                }
            }
            if (!(unlockGasMeter || unlockElectricityMeter))
            {
                UnlockSuccess();
                return;
            }

            Dispatcher.Dispatch(new StoreUnlockedMeterDataAction {
                UnlockGasMeter = unlockGasMeter,
                UnlockElectricityMeter = unlockElectricityMeter,
                ElectricityMeterMpan = newMpan,
                GasMeterMprn = newMprn
            });

            UnlockSuccess();
            return;
        }
        catch (Exception ex)
        {
            unlockingError = true;
            Logger.LogError(ex, "Error locking data, attempting to rollback");
            // #TODO: Rollback using dispatch, unlock whole app and show error messages for the states that failed to unlock. Maybe split up this method and unlock each state separately.
            // 

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
            unlocking = false;
        }
    }

    bool unlockingError = false;

    void UnlockSuccess()
    {
        model.Input = null;
        Dispatcher.Dispatch(new SetSetupDataUnlockedAction());
        unlocking = false;
    }
}
