using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Energy.App.Standalone.Features.Setup.Meter.Pages
{
    public partial class MeterSetupTable
    {
        [Inject] private ISnackbar Snackbar { get; set; }

        //[Inject] UserState UserState { get; set; }
        [Inject]
        private NavigationManager NavManager { get; set; }

        [Inject] private IState<HouseholdState> HouseholdState { get; set; }
        [Inject] private IState<GasReadingsState> GasReadingsState { get; set; }
        [Inject] private IState<ElectricityReadingsState> ElectricityReadingsState { get; set; }

        [Inject] private IState<MeterSetupState> MeterSetupState { get; set; }



        //[Inject] IMeterApi MeterApi { get; set; }
        //[Inject] IEnergyDataProcessor EnergyDataProcessor { get; set; }
        [Inject]
        private IDialogService DialogService { get; set; }

        private Dictionary<MeterType, MeterStatus> _meterStatusList;
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

            SubscribeToAction<NotifyElectricityLoadingFinished>((a) =>
            {

                Snackbar.Add("Electricity Readings Updated");

            });
            SubscribeToAction<NotifyGasLoadingFinished>((a) =>
            {
                Snackbar.Add(!String.IsNullOrEmpty(a.Error) ? "Gas Readings Update Error" : "Gas Readings Updated");
            });

            SubscribeToAction<GasDeleteReadingsAction>((a) =>
            {
                Snackbar.Add("Gas Readings Deleted");

            });

            SubscribeToAction<ElectricityDeleteReadingsAction>((a) =>
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


            SetMeterStatusList();

        }


        private void SetMeterStatusList()
        {
            _meterStatusList = MeterSetupState.Value.MeterStates.Select(MeterStateToStatus).ToDictionary(status => status.MeterType);
        }

        private MeterStatus MeterStateToStatus(MeterState meterState)
        {
            TariffInfo tariffInfo = GetTariffInfo(meterState);

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
                MeterType.Electricity => ElectricityReadingsState.Value.CostedReadings?.LastOrDefault()?.UtcTime.Date,
                MeterType.Gas => GasReadingsState.Value.CostedReadings?.LastOrDefault()?.UtcTime.Date,
                _ => throw new ArgumentOutOfRangeException(nameof(meterType)),
            };
        }

        private bool ReadingsStatus(MeterType meterType)
        {
            return meterType switch
            {
                MeterType.Electricity => ElectricityReadingsState.Value.Loading,
                MeterType.Gas => GasReadingsState.Value.Loading,
                _ => throw new ArgumentOutOfRangeException(nameof(meterType)),
            };
        }


        private static TariffInfo GetTariffInfo(MeterState meterState)
        {
            List<TariffDetailState> tariffDetailState = meterState.TariffDetails.ToList();


            string tariffUnitRateText = "No active tariff set";
            bool hasAnyTariffs = tariffDetailState?.Any() ?? false;
            TariffDetailState currentTariff = null;
            if (hasAnyTariffs)
            {
                currentTariff = tariffDetailState.eCurrentTariffState();
            }

            string tariffStandingChargeText = string.Empty;
            if (currentTariff != null)
            {
                tariffUnitRateText = currentTariff.eTariffUnitRateText();
                tariffStandingChargeText = $"Standing Charge: {currentTariff.DailyStandingChargePence:N0}p/Day";
            }

            bool hasActiveTariff = hasAnyTariffs && currentTariff != null;

            return new TariffInfo()
            {
                HasActiveTariff = hasActiveTariff,
                TariffUnitRateText = tariffUnitRateText,
                TariffStandingChargeText = tariffStandingChargeText
            };
        }

        private MudMessageBox DeleteBox { get; set; }

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
                    Dispatcher.Dispatch(new DeleteGasAction());
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new DeleteElectricityAction());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType));
                    
            }
        }

        protected async void DispatchUpdateReadings(MeterType meterType)
        {

            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(false));;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType));
            }
        }

        private bool Reloading = false;

        protected async void DispatchReloadReadings(MeterType meterType)
        {
            var reloadCompletion = new TaskCompletionSource<int>();
            Reloading = true;

            switch (meterType)
            {
                case MeterType.Gas:
                    Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(true, reloadCompletion));
                    break;
                case MeterType.Electricity:
                    Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(true, reloadCompletion));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType));
            }
            if (!reloadCompletion.Task.IsCompleted)
            {
                await reloadCompletion.Task;
            }

            Reloading = false;
            await InvokeAsync(StateHasChanged);
        }

    }

    public record TariffInfo()
    {
        public bool HasActiveTariff {get; init;}
        public string TariffUnitRateText {get; init;}  
        public string TariffStandingChargeText {get; init;}
    }
}