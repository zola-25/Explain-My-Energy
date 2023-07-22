using Energy.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Fluxor;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using System.Collections.Immutable;
using Energy.App.Standalone.PageComponents;

namespace Energy.App.Standalone.Pages
{
    public partial class Index
    {
        [Inject] IState<HouseholdState> HouseholdState { get; set; }
        [Inject] IState<MeterSetupState> MeterSetupState { get; set; }
        [Inject] IState<ElectricityReadingsState> ElectricityReadingsState { get; set; }
        [Inject] IState<GasReadingsState> GasReadingsState { get; set; }
        [Inject] IState<HistoricalForecastState> HistoricalForecastState { get; set; }
        [Inject] IState<HeatingForecastState> HeatingForecastState { get; set; }
        
        [Inject] InMemoryStateContainer InMemoryStateContainer { get; set; }
        bool Ready;

        bool HouseholdSetupValid;
        string HouseholdStatus;
        Severity HouseholdSeverity;

        bool GasMeterSetupValid;
        Severity GasSeverity;
        string GasStatus;


        bool ElectricityMeterSetupValid;
        string ElectricityStatus;
        Severity ElectricitySeverity;



        (bool Valid, string Status) ElectricityReadingsStatus;
        (bool Valid, string Status) GasReadingsStatus;

        Severity ElectricityReadingsSeverity;
        Severity GasReadingsSeverity;

        (bool Valid, string Status) GasHistoricalForecastStatus;
        Severity GasHistoricalForecastSeverity;

        (bool Valid, string Status) ElectricityHistoricalForecastStatus;
        Severity ElectricityHistoricalForecastSeverity;

        (bool Valid, string Status) HeatingForecastStatus;

        Severity HeatingForecastSeverity;

        SetupStage SetupStage;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Ready = false;
            SetupStage = SetupStage.None;

            SetupStage |= InMemoryStateContainer.WelcomeTermsAccepted ? 0 : SetupStage.NotSeenWelcomeScreenSplash;

            HouseholdSetupValid = !HouseholdState.Value.Invalid && HouseholdState.Value.Saved;
            HouseholdStatus = HouseholdSetupValid ? " Valid" : "Setup Required";
            HouseholdSeverity = HouseholdSetupValid ? Severity.Success : Severity.Warning;

            SetupStage |= HouseholdSetupValid ? 0 : SetupStage.HouseholdNotSetup;

            var gasMeterState = MeterSetupState.Value[MeterType.Gas];
            SetupStage |= gasMeterState.InitialSetupValid ? 0 : SetupStage.GasMeterNotSetup;

            SetupStage |= gasMeterState.Authorized ? 0 : SetupStage.GasMeterNotAuthorized;


            GasMeterSetupValid = gasMeterState.SetupValid;
            GasStatus = GasMeterSetupValid ? "Setup Valid" : "Setup Required";
            GasSeverity = GasMeterSetupValid ? Severity.Success : Severity.Warning;

            var electricityMeterState = MeterSetupState.Value[MeterType.Electricity];

            SetupStage |= electricityMeterState.InitialSetupValid ? 0 : SetupStage.ElectricityMeterNotSetup;
            SetupStage |= electricityMeterState.Authorized ? 0 : SetupStage.ElectricityMeterNotAuthorized;

            bool defaultOpenWizard = ( SetupStage & SetupStage.NotSeenWelcomeScreenSplash ) != 0;
            OpenWizard = OpenWizard || defaultOpenWizard;
            ElectricityMeterSetupValid = electricityMeterState.SetupValid;
            ElectricityStatus = ElectricityMeterSetupValid ? "Setup Valid" : "Setup Required";
            ElectricitySeverity = ElectricityMeterSetupValid ? Severity.Success : Severity.Warning;


            ElectricityReadingsStatus = GetMeterReadingsStatus(MeterType.Electricity);
            GasReadingsStatus = GetMeterReadingsStatus(MeterType.Gas);

            ElectricityReadingsSeverity = ElectricityReadingsStatus.Valid ? Severity.Success : Severity.Warning;
            GasReadingsSeverity = GasReadingsStatus.Valid ? Severity.Success : Severity.Warning;

            ElectricityHistoricalForecastStatus = GetHistoricalForecastStatus(MeterType.Electricity);
            GasHistoricalForecastStatus = GetHistoricalForecastStatus(MeterType.Gas);

            ElectricityHistoricalForecastSeverity = ElectricityHistoricalForecastStatus.Valid ? Severity.Success : Severity.Warning;
            GasHistoricalForecastSeverity = GasHistoricalForecastStatus.Valid ? Severity.Success : Severity.Warning;

            HeatingForecastStatus = GetHeatingForecastStatus();
            HeatingForecastSeverity = HeatingForecastStatus.Valid ? Severity.Success : Severity.Warning;
            Ready = true;
        }

        
        (bool Valid, string Status) GetMeterReadingsStatus(MeterType meterType)
        {
            if (!MeterSetupState.Value[meterType].SetupValid)
            {
                return (false, "Requires Meter Setup");
            }

            var costedReadings = GetCostedReadings(meterType);
            if (costedReadings.eIsNullOrEmpty())
            {
                return (false, "No costed reading calculated");
            }

            var first = costedReadings.First().UtcTime;
            var last = costedReadings.Last().UtcTime;
            string rangeText = $"Data available from {first.eDateToDowShortMonthYY()} to {last.eDateToDowShortMonthYY()}";
            return (true, rangeText);
        }




        (bool Valid, string Status) GetHistoricalForecastStatus(MeterType meterType)
        {
            if (!MeterSetupState.Value[meterType].SetupValid)
            {
                return (false, "Requires Meter Setup");
            }

            var readings = GetBasicReadings(meterType);
            if (readings.eIsNullOrEmpty())
            {
                return (false, $"No {meterType} readings available");
            }

            if (readings.Count < 180 * 48)
            {
                return (false, $@"Ideally require at least 180 days of historical readings for historical forecasting.
                                Currently first reading is {readings.First().UtcTime.eDateToDowShortMonthYY()}");
            }

            if (HistoricalForecastState.Value[meterType].eIsNullOrEmpty())
            {
                return (false, $"No historical forecast calculated for {meterType} Meter");
            }

            return (true, $"From {HistoricalForecastState.Value[meterType].First().UtcTime.eDateToDowShortMonthYY()} to {HistoricalForecastState.Value[meterType].Last().UtcTime.eDateToDowShortMonthYY()}");
        }


        private (bool Valid, string Status) GetHeatingForecastStatus()
        {
            var heatSourceMeterType = HouseholdState.Value.PrimaryHeatSource;
            var (Valid, Status) = GetMeterReadingsStatus(heatSourceMeterType);
            if (!Valid)
            {
                return (false, Status);
            }

            if (!HeatingForecastState.Value.SavedCoefficients)
            {
                return (false, "Heating forecast coefficients not inititalized");
            }

            if (HeatingForecastState.Value.ForecastWeatherReadings.eIsNullOrEmpty())
            {
                return (false, "Forecast weather data not loaded");
            }

            if (HeatingForecastState.Value.ForecastDailyCosts.eIsNullOrEmpty())
            {
                return (false, "Forecast daily costs not loaded");
            }

            return (true, $@"Heating forecast range: {HeatingForecastState.Value.ForecastDailyCosts.First().UtcTime.eDateToDowShortMonthYY()}
                        to {HeatingForecastState.Value.ForecastDailyCosts.Last().UtcTime.eDateToDowShortMonthYY()}");
        }

        private ImmutableList<CostedReading> GetCostedReadings(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.CostedReadings,
                MeterType.Gas => GasReadingsState.Value.CostedReadings,
                _ => throw new NotImplementedException(),
            };
        }

        private ImmutableList<BasicReading> GetBasicReadings(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.BasicReadings,
                MeterType.Gas => GasReadingsState.Value.BasicReadings,
                _ => throw new NotImplementedException(),
            };
        }

        
    }
}