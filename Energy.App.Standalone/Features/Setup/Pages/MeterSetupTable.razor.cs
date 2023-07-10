using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
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
        [Inject] IState<GasReadingsState> GasReadingsState { get; set; }
        [Inject] IState<ElectricityReadingsState> ElectricityReadingsState { get; set; }
        [Inject] IState<ElectricityTariffsState> ElectricityTariffsState { get; set; }
        [Inject] IState<GasTariffsState> GasTariffsState { get; set; }

        [Inject] IState<MeterSetupState> MeterSetupState { get; set; }



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

            });
            SubscribeToAction<NotifyGasStoreReady>((a) =>
            {
                Snackbar.Add("Gas Readings Updated");
            });

            SubscribeToAction<NotifyGasReadingsDeletedAction>((a) =>
            {
                Snackbar.Add("Gas Readings Deleted");

            });

            SubscribeToAction<NotifyElectricityReadingsDeletedAction>((a) =>
            {
                Snackbar.Add("Electricity Readings Deleted");

            });
            
            HouseholdState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            MeterSetupState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            GasReadingsState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            ElectricityReadingsState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            GasTariffsState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            ElectricityTariffsState.StateChanged += async (sender, state) =>
            {
                SetMeterStatusList();
                await InvokeAsync(StateHasChanged);
            };
            
            SetMeterStatusList();

        }





        private void SetMeterStatusList()
        {
            _meterStatusList = MeterSetupState.Value.MeterStates.Select(MeterStateToStatus).ToDictionary(status => status.MeterType);
        }

        private MeterStatus MeterStateToStatus(MeterState meterState)
        {
            TariffInfo tariffInfo = GetTariffInfo(meterState.MeterType);

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
                MeterType.Electricity => ElectricityReadingsState.Value.CostedReadings.LastOrDefault()?.UtcTime.Date,
                MeterType.Gas => GasReadingsState.Value.CostedReadings.LastOrDefault()?.UtcTime.Date,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private bool ReadingsStatus(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.ReloadingReadings || ElectricityReadingsState.Value.UpdatingReadings,
                MeterType.Gas => GasReadingsState.Value.ReloadingReadings || GasReadingsState.Value.UpdatingReadings,
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
                    Dispatcher.Dispatch(new GasUpdateReadingsAndCostsAction(GasReadingsState.Value.CostedReadings.Last().UtcTime.Date));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new ElectricityUpdateReadingsAndCostsAction(ElectricityReadingsState.Value.CostedReadings.Last().UtcTime.Date));
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
                    Dispatcher.Dispatch(new GasReloadReadingsAndCostsAction());
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new ElectricityReloadReadingsAndCostsAction());
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