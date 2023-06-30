using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.Metrics;

namespace Energy.App.Standalone.Features.Setup.Pages
{
    public partial class MeterSetupTable
    {
        [Inject] ISnackbar Snackbar { get; set; }

        //[Inject] UserState UserState { get; set; }
        [Inject]
        NavigationManager NavManager { get; set; }

        [Inject] IState<HouseholdState> HouseholdState { get; set; }
        [Inject] IState<GasReadingsState> GasReadingsState { get; set; }
        [Inject] IState<ElectricityReadingsState> ElectricityReadingsState { get; set; }
        [Inject] IState<ElectricityTariffsState> ElectricityTariffsState { get; set; }
        [Inject] IState<GasTariffsState> GasTariffsState { get; set; }




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
            public bool ReadingsLoading { get; init; }
            public DateTime? LatestReadingsDate { get; init; }
            public bool HasActiveTariff { get; init; }
            public string TariffUnitRateText { get; init; }
            public string TariffStandingChargeText { get; set; }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            SubscribeToAction<NotifyElectricityStoreReady>((a) =>
            {
                Snackbar.Add("Electricity Readings Updated");

                SetMeterStatusList();
            });
            SubscribeToAction<NotifyGasStoreReady>((a) =>
            {
                Snackbar.Add("Gas Readings Updated");
                SetMeterStatusList();
            });

            SubscribeToAction<NotifyGasReadingsDeletedAction>((a) =>
            {
                Snackbar.Add("Gas Readings Deleted");

                SetMeterStatusList();
            });

            SubscribeToAction<NotifyElectricityReadingsDeletedAction>((a) =>
            {
                Snackbar.Add("Electricity Readings Deleted");

                SetMeterStatusList();
            });

            SubscribeToAction<NotifyGasStoreReady>((a) =>
            {
                Snackbar.Add("Gas Readings Updated");
                SetMeterStatusList();
            });

            SubscribeToAction<GasReloadReadingsAction>((a) => SetMeterStatusList());
            SubscribeToAction<ElectricityReloadReadingsAction>((a) => SetMeterStatusList());

            SubscribeToAction<GasUpdateReadingsAction>((a) => SetMeterStatusList());
            SubscribeToAction<ElectricityUpdateReadingsAction>((a) => SetMeterStatusList());

            SetMeterStatusList();

        }


        protected override void OnParametersSet()
        {
            //SubscribeToAction<ElectricityReloadReadingsAction>(ShowMeterAddedToast);
            //SubscribeToAction<ElectricityUpdateReadingsAction>(ShowMeterUpdatedToast);
            //SubscribeToAction<GasReloadReadingsAction>(ShowMeterUpdatedToast);
            //SubscribeToAction<GasUpdateReadingsAction>(ShowMeterUpdatedToast);
            //SubscribeToAction<GasReloadReadingsAction>(ShowMeterUpdatedToast);

            //SubscribeToAction<MeterDeleteAction>(ShowMeterDeletedToast);
        }



        private void SetMeterStatusList()
        {
            _meterStatusList = MeterSetupState.Value.MeterStates.Select(MeterStateToStatus).ToDictionary(status => status.MeterType);
        }

        private MeterStatus MeterStateToStatus(MeterState meterState)
        {
            var tariffInfo = GetTariffInfo(meterState.MeterType);

            return new MeterStatus()
            {
                Added = meterState.InitialSetupValid,
                Authorized = meterState.Authorized,
                ReadingsLoading = ReadingsStatus(meterState.MeterType),
                LatestReadingsDate = ReadingsLatestDate(meterState.MeterType),
                GlobalId = meterState.GlobalId,
                MeterType = meterState.MeterType,
                Mpxn = meterState.Mpxn,
                HasActiveTariff = tariffInfo.HasActiveTariff,
                TariffUnitRateText = tariffInfo.TariffUnitRateText,
                TariffStandingChargeText = tariffInfo.TariffStandingChargeText
            };
        }

        private DateTime? ReadingsLatestDate(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.BasicReadings.LastOrDefault()?.LocalTime.Date,
                MeterType.Gas => GasReadingsState.Value.BasicReadings.LastOrDefault()?.LocalTime.Date,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private bool ReadingsStatus(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.Reloading || ElectricityReadingsState.Value.Updating,
                MeterType.Gas => GasReadingsState.Value.Reloading || GasReadingsState.Value.Updating,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }


        private TariffInfo GetTariffInfo(MeterType meterType)
        {
            List<TariffDetailState> tariffDetailState;
            switch (meterType)
            {
                case MeterType.Gas:
                    tariffDetailState = GasTariffsState.Value.TariffDetails.ToList();
                    break;
                case MeterType.Electricity:
                    tariffDetailState = ElectricityTariffsState.Value.TariffDetails.ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string tariffUnitRateText = "No active tariff set";
            bool hasAnyTariffs = tariffDetailState?.Any() ?? false;
            TariffDetailState currentTariff = null;
            if (hasAnyTariffs)
            {
                currentTariff = tariffDetailState.eCurrentTariffState();
            }

            string tariffStandingChargeText = String.Empty;
            if (currentTariff != null)
            {
                tariffUnitRateText = currentTariff.eTariffUnitRateText();
                tariffStandingChargeText = $"Standing Charge: {currentTariff.DailyStandingChargePence:N0}p/Day";
            }

            bool hasActiveTariff = hasAnyTariffs && currentTariff != null;

            return new TariffInfo(hasActiveTariff, tariffUnitRateText, tariffStandingChargeText);
        }

        MudMessageBox DeleteBox { get; set; }

        private async Task RemoveMeterAsync(Guid globalId, MeterType meterType)
        {
            bool? result = await DeleteBox.Show();
            if (result == null || result == false)
            {
                return;
            }

            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new GasMeterDeleteAction());
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new ElectricityMeterDeleteAction());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void DispatchUpdateReadings(MeterType meterType)
        {
            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new GasUpdateReadingsAction(GasReadingsState.Value.BasicReadings.Last().LocalTime.Date));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new ElectricityUpdateReadingsAction(ElectricityReadingsState.Value.BasicReadings.Last().LocalTime.Date));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void DispatchReloadReadings(MeterType meterType)
        {
            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new GasReloadReadingsAction());
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new ElectricityReloadReadingsAction());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

    public record TariffInfo(bool HasActiveTariff, string TariffUnitRateText, string TariffStandingChargeText)
    {
        
    }
}