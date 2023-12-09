using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Pages.SetupWizard;
using Energy.Shared;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Setup.TermsAndConditions;
using Energy.App.Standalone.Features.Setup.Household.Actions;

namespace Energy.App.Standalone.PageComponents;
public partial class AppSetupStatus
{
    [Inject] IState<TermsAndConditionsState> TermsAndConditionsState { get; set; }
    [Inject] IState<HouseholdState> HouseholdState { get; set; }
    [Inject] IState<MeterSetupState> MeterSetupState { get; set; }
    [Inject] IState<ElectricityReadingsState> ElectricityReadingsState { get; set; }
    [Inject] IState<GasReadingsState> GasReadingsState { get; set; }
    [Inject] IState<HistoricalForecastState> HistoricalForecastState { get; set; }
    [Inject] IState<HeatingForecastState> HeatingForecastState { get; set; }



    [Parameter] public EventHandler<(MeterType, bool)> HeatingAnalysisValidCallback { get; set; }
    [Parameter] public EventHandler<(MeterType, bool)> HistoricalAnalysisValidCallback { get; set; }

    bool Ready;

    bool OpenWizard = false;

    bool HouseholdSetupValid => !HouseholdState.Value.Invalid && HouseholdState.Value.Saved;
    string HouseholdStatus => HouseholdSetupValid ? " Valid" : "Setup Required";
    Severity HouseholdSeverity => HouseholdSetupValid ? Severity.Success : Severity.Warning;

    bool GasMeterSetupValid => MeterSetupState.Value[MeterType.Gas].SetupValid;
    Severity GasSeverity => GasMeterSetupValid ? Severity.Success : Severity.Warning;
    string GasStatus => GasMeterSetupValid ? "Setup Valid" : MeterSetupState.Value[MeterType.Gas].InitialSetupValid ? "Authorization Required" : "Setup Required";


    bool ElectricityMeterSetupValid => MeterSetupState.Value[MeterType.Electricity].SetupValid;
    string ElectricityStatus => ElectricityMeterSetupValid ? "Setup Valid" : MeterSetupState.Value[MeterType.Electricity].InitialSetupValid ? "Authorization Required" : "Setup Required";
    Severity ElectricitySeverity => ElectricityMeterSetupValid ? Severity.Success : Severity.Warning;



    (bool Valid, string Status) ElectricityReadingsStatus => GetMeterReadingsStatus(MeterType.Electricity);
    (bool Valid, string Status) GasReadingsStatus => GetMeterReadingsStatus(MeterType.Gas);

    Severity ElectricityReadingsSeverity => ElectricityReadingsStatus.Valid ? Severity.Success : Severity.Warning;
    Severity GasReadingsSeverity => GasReadingsStatus.Valid ? Severity.Success : Severity.Warning;

    (bool Valid, string Status) GasHistoricalForecastStatus => GetHistoricalForecastStatus(MeterType.Gas);
    Severity GasHistoricalForecastSeverity => GasHistoricalForecastStatus.Valid ? Severity.Success : Severity.Warning;

    (bool Valid, string Status) ElectricityHistoricalForecastStatus => GetHistoricalForecastStatus(MeterType.Electricity);
    Severity ElectricityHistoricalForecastSeverity => ElectricityHistoricalForecastStatus.Valid ? Severity.Success : Severity.Warning;

    (bool Valid, string Status) HeatingForecastStatus => GetHeatingForecastStatus();

    Severity HeatingForecastSeverity => HeatingForecastStatus.Valid ? Severity.Success : Severity.Warning;


    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Ready = false;
        if (UserLockState.Value.LockingOrLocked)
        {
            Ready = true;
            return;
        }

        OpenWizard = OpenWizard || !TermsAndConditionsState.Value.WelcomeScreenSeenAndDismissed;

        Ready = true;
    }


    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
            return;
        var heatingMeter = HouseholdState.Value.PrimaryHeatSource;

        switch (heatingMeter)
        {
            case MeterType.Gas:
                HeatingAnalysisValidCallback?.Invoke(this, (MeterType.Gas, HeatingForecastStatus.Valid));
                HistoricalAnalysisValidCallback?.Invoke(this, (MeterType.Electricity, ElectricityHistoricalForecastStatus.Valid));
                break;
            case MeterType.Electricity:
                HeatingAnalysisValidCallback?.Invoke(this, (MeterType.Electricity, HeatingForecastStatus.Valid));
                HistoricalAnalysisValidCallback?.Invoke(this, (MeterType.Gas, GasHistoricalForecastStatus.Valid));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(heatingMeter), heatingMeter, "Meter not recognised");
        }
    }

    (bool Valid, string Status) GetMeterReadingsStatus(MeterType meterType)
    {
        if (!MeterSetupState.Value[meterType].InitialSetupValid)
        {
            return (false, "Requires Meter Setup");
        }
        if (!MeterSetupState.Value[meterType].SetupValid)
        {
            return (false, "Requires Meter Authorization");
        }

        var costedReadings = GetCostedReadings(meterType);
        if (costedReadings.eIsNullOrEmpty())
        {
            return (false, "No costed reading calculated");
        }

        var first = costedReadings.First().UtcTime;
        var last = costedReadings.Last().UtcTime;
        string rangeText = $"Data available {first.eDateToDowShortMonthYY()} - {last.eDateToDowShortMonthYY()}";
        return (true, rangeText);
    }




    (bool Valid, string Status) GetHistoricalForecastStatus(MeterType meterType)
    {
        if (!MeterSetupState.Value[meterType].InitialSetupValid)
        {
            return (false, "Requires Meter Setup");
        }

        if (!MeterSetupState.Value[meterType].SetupValid)
        {
            return (false, "Requires Meter Authorization");
        }

        var readings = GetBasicReadings(meterType);
        if (readings.eIsNullOrEmpty())
        {
            return (false, $"No {meterType} readings available");
        }

        if (readings.Count < 180 * 48)
        {
            return (false, $@"Ideally require at least 180 days of historical readings for historical forecasting.
                                Currently first reading is {readings.First().Utc.eDateToDowShortMonthYY()}");
        }

        if (HistoricalForecastState.Value[meterType].eIsNullOrEmpty())
        {
            return (false, $"No historical forecast calculated for {meterType} Meter");
        }

        return (true, $"Forecast range available {HistoricalForecastState.Value[meterType].First().UtcTime.eDateToDowShortMonthYY()} - {HistoricalForecastState.Value[meterType].Last().UtcTime.eDateToDowShortMonthYY()}");
    }

    
    private (bool Valid, string Status) GetHeatingForecastStatus() {
        var heatSourceMeterType = HouseholdState.Value.PrimaryHeatSource;

        (bool valid, string status) = GetHeatingForecastStatusWithoutMeterName(heatSourceMeterType);
        
        string statusText = GetMeterHeatingForecastStatusText(heatSourceMeterType, status);
        
        return (valid, statusText);
    }

    private string GetMeterHeatingForecastStatusText(MeterType meterType, string appendingStatusText)
    {
        return $"Temperature-based Forecast ({meterType}): {appendingStatusText}";
    }

    private (bool valid, string status) GetHeatingForecastStatusWithoutMeterName(MeterType heatSourceMeterType)
    {
        (bool valid, string meterReadingsStatus) = GetMeterReadingsStatus(heatSourceMeterType);
        if (!valid)
        {
            return (false, meterReadingsStatus);
        }

        if (!HeatingForecastState.Value.SavedCoefficients)
        {
            return (false, "Heating forecast coefficients not initialized");
        }

        if (HeatingForecastState.Value.ForecastWeatherReadings.eIsNullOrEmpty())
        {
            return (false, "Forecast weather data not loaded");
        }

        if (HeatingForecastState.Value.ForecastDailyCosts.eIsNullOrEmpty())
        {
            return (false, "Forecast daily costs not loaded");
        }

        return (true, $@"Forecast range available {HeatingForecastState.Value.ForecastDailyCosts.First().UtcTime.eDateToDowShortMonthYY()}
                        - {HeatingForecastState.Value.ForecastDailyCosts.Last().UtcTime.eDateToDowShortMonthYY()}");
    }

    private ImmutableList<CostedReading> GetCostedReadings(MeterType meterType)
    {
        return meterType switch {
            MeterType.Electricity => ElectricityReadingsState.Value.CostedReadings,
            MeterType.Gas => GasReadingsState.Value.CostedReadings,
            _ => throw new NotImplementedException(),
        };
    }

    private ImmutableList<BasicReading> GetBasicReadings(MeterType meterType)
    {
        return meterType switch {
            MeterType.Electricity => ElectricityReadingsState.Value.BasicReadings,
            MeterType.Gas => GasReadingsState.Value.BasicReadings,
            _ => throw new NotImplementedException(),
        };
    }
}
