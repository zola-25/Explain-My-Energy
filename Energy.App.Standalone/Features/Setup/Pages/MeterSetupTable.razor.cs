using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Energy.App.Standalone.Features.Setup.Pages
{
    public partial class MeterSetupTable
    {
        [Inject] ISnackbar Snackbar { get; set; }

        //[Inject] UserState UserState { get; set; }
        [Inject]
        NavigationManager NavManager { get; set; }

        [Inject] IState<HouseholdState> HouseholdState { get; set; }


        //[Inject] IMeterApi MeterApi { get; set; }
        //[Inject] IEnergyDataProcessor EnergyDataProcessor { get; set; }
        [Inject]
        private IDialogService DialogService { get; set; }

        Dictionary<MeterType, MeterStatus> _meterStatusList;
        private record MeterStatus
        {
            public Guid GlobalId { get; init; }
            public MeterType MeterType { get; init; }
            public bool Added { get; init; }
            public string Mpxn { get; init; }
            public bool Authorized { get; init; }
            public bool Loading { get; init; }
            public bool HasActiveTariff { get; init; }
            public string TariffUnitRateText { get; init; }
            public string TariffStandingChargeText { get; set; }
        }

        protected override void OnParametersSet()
        {
            SubscribeToAction<ElectricityMeterInitialAddAction>(ShowMeterAddedToast);
            SubscribeToAction<ElectricityMeterInitialUpdateAction>(ShowMeterUpdatedToast);
            SubscribeToAction<MeterDeleteAction>(ShowMeterDeletedToast);
            SetMeterStatusList();
        }

        private void ShowMeterDeletedToast(MeterDeleteAction action)
        {
            Snackbar.Add("Meter Deleted", Severity.Info);
        }

        private void ShowMeterUpdatedToast(ElectricityMeterInitialUpdateAction action)
        {
            Snackbar.Add("Meter Updated", Severity.Success);

        }

        private void ShowMeterAddedToast(ElectricityMeterInitialAddAction action)
        {
            Snackbar.Add("Meter Added", Severity.Success);

        }


        private void SetMeterStatusList()
        {
            _meterStatusList = MeterSetupState.Value.MeterStates.Select(MeterStateToStatus).ToDictionary(status => status.MeterType);
        }

        private MeterStatus MeterStateToStatus(MeterState meterState)
        {
            string tariffUnitRateText = "No active tariff set";
            string tariffStandingChargeText = String.Empty;
            return new MeterStatus()
            {
                Added = meterState.GlobalId != Guid.Empty,
                Authorized = meterState.Authorized,
                Loading = false,
                GlobalId = meterState.GlobalId,
                MeterType = meterState.MeterType,
                Mpxn = meterState.Mpxn,
                HasActiveTariff = false,
                TariffUnitRateText = tariffUnitRateText,
                TariffStandingChargeText = tariffStandingChargeText
            };
        }

        [Obsolete("Not used for now")]
        private MeterStatus MeterToStatus(Meter meter)
        {
            string tariffUnitRateText = "No active tariff set";
            bool hasAnyTariffs = meter.TariffDetails?.Any() ?? false;
            TariffDetail currentTariff = null;
            if (hasAnyTariffs)
            {
                currentTariff = meter.TariffDetails.eCurrentTariff();
            }

            string tariffStandingChargeText = String.Empty;
            if (currentTariff != null)
            {
                tariffUnitRateText = currentTariff.eTariffUnitRateText();
                tariffStandingChargeText = $"Standing Charge: {currentTariff.DailyStandingChargePence:N0}p/Day";
            }

            return new MeterStatus()
            {
                Added = meter.GlobalId != Guid.Empty,
                Authorized = meter.Authorized,
                Loading = meter.QueueFreshImport,
                GlobalId = meter.GlobalId,
                MeterType = meter.MeterType,
                Mpxn = meter.Mpxn,
                HasActiveTariff = hasAnyTariffs && currentTariff != null,
                TariffUnitRateText = tariffUnitRateText,
                TariffStandingChargeText = tariffStandingChargeText
            };
        }

        MudMessageBox DeleteBox { get; set; }

        private async Task RemoveMeterAsync(Guid globalId, MeterType meterType)
        {
            bool? result = await DeleteBox.Show();
            if (result == null || result == false)
            {
                return;
            }

            Dispatcher.Dispatch(new MeterDeleteAction(meterType, globalId));
        }

        private async Task ImportData(Guid globalId)
        {
            Snackbar.Add("Cannot Import Yet", Severity.Info);

            //var meter = MeterState[globalId];
            //if (meter.Authorized)
            //{
            //    meter.QueueFreshImport = true;
            //    _meterStatusList[meter.MeterType] = MeterToStatus(meter);
            //    StateHasChanged();
            //    await EnergyDataProcessor.ReloadData(meter);
            //}

            //meter.QueueFreshImport = false;
            //_meterStatusList[meter.MeterType] = MeterToStatus(meter);
            //StateHasChanged();
        }
    }
}