using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class AppInitEffects
    {
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;

        public AppInitEffects(IState<HouseholdState> householdState,
            IState<WeatherState> weatherState,
            IState<MeterSetupState> meterSetupState,
            IState<GasReadingsState> gasReadingsState,
            IState<ElectricityReadingsState> electricityReadingsState)
        {
            _householdState = householdState;
            _weatherState = weatherState;
            _meterSetupState = meterSetupState;
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
        }

        // [EffectMethod]
        // public async Task HandleInitializeAppAction(InitializeAppAction action, IDispatcher dispatcher)
        // {
        //     bool canUpdateWeatherData = _householdState.Value.Saved;
        //     bool canUpdateElectricityData = _meterSetupState.Value.ElectricityMeter.SetupValid;
        //     bool canUpdateGasData = _meterSetupState.Value.GasMeter.SetupValid;
        //     bool canUpdateLinearCoefficients = _meterSetupState.Value[_householdState.Value.PrimaryHeatSource].
        //         SetupValid;
        //
        //     dispatcher.Dispatch(new NotifyAppStartedAction());
        //
        //     dispatcher.Dispatch(new InitiateAppInitUpdateWeatherDataAction(canUpdateWeatherData));
        //     dispatcher.Dispatch(new InititateAppInitUpdateElectricityReadingsAction(canUpdateElectricityData));
        //     dispatcher.Dispatch(new InitiateAppInitUpdateGasReadingsAction(canUpdateGasData));
        //     dispatcher.Dispatch(new InitiateAppInitUpdateLinearCoefficientsAction(canUpdateLinearCoefficients));
        // }

        [EffectMethod]
        public async Task HandleAppInitWeatherAction(InitiateAppInitUpdateWeatherDataAction action, IDispatcher dispatcher)
        {
            if (!action.CanUpdateWeatherData)
            {
                dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction());
                return;
            }

            if (!_weatherState.Value.WeatherReadings.Any())
            {
                dispatcher.Dispatch(new InitiateWeatherReloadReadingsAction(_householdState.Value.OutCodeCharacters));
                return;
            }

            var latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.
                UtcReadDate;
            var latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.
                UtcReadDate;

            if (latestReading < DateTime.UtcNow.Date.AddDays(-1))
            {
                dispatcher.Dispatch
                (
                    new InitiateWeatherUpdateReadingsAction
                    (
                        _householdState.Value.OutCodeCharacters,
                        latestReading,
                        latestHistoricalReading
                    )
                );
            }
            else
            {
                await Task.Delay(5000);
                dispatcher.Dispatch(new NotifyWeatherReadingsReadyAction());
            }
        }

        [EffectMethod]
        public async Task HandleAppInitElectricityAction(InititateAppInitUpdateElectricityReadingsAction action, IDispatcher dispatcher)
        {
            if (!action.CanUpdateElectricityReadingsData)
            {
                return;
            }

            await Task.Run
            (
                () =>
                {
                    if (_electricityReadingsState.Value.BasicReadings.Any())
                    {
                        DateTime lastReading = _electricityReadingsState.Value.BasicReadings.Last().
                            UtcTime;
                        if (lastReading < DateTime.Today.AddDays(-1))
                        {
                            dispatcher.Dispatch(new ElectricityUpdateReadingsAction(lastReading.Date));
                        }
                    }
                    else
                    {
                        dispatcher.Dispatch(new ElectricityReloadReadingsAction());
                    }
                }
            );


            dispatcher.Dispatch(new ElectricityInitiateCostCalculationsAction());


            if (_householdState.Value.PrimaryHeatSource == MeterType.Electricity)
            {
                dispatcher.Dispatch(new InitiateUpdateLinearCoefficientsAction());
            }
        }

        [EffectMethod]
        public async Task HandleAppInitGasAction(InitiateAppInitUpdateGasReadingsAction action, IDispatcher dispatcher)
        {
            if (!action.CanUpdateGasReadingsData)
            {
                return;
            }

            await Task.Run
            (
                () =>
                {
                    if (_gasReadingsState.Value.BasicReadings.Any())
                    {
                        DateTime lastReading = _gasReadingsState.Value.BasicReadings.Last().
                            UtcTime;
                        if (lastReading < DateTime.Today.AddDays(-1))
                        {
                            dispatcher.Dispatch(new GasUpdateReadingsAction(lastReading.Date));
                        }
                    }
                    else
                    {
                        dispatcher.Dispatch(new GasReloadReadingsAction());
                    }
                }
            );


            dispatcher.Dispatch(new GasInitiateCostCalculationsAction());

            if (_householdState.Value.PrimaryHeatSource == MeterType.Gas)
            {
                dispatcher.Dispatch(new InitiateUpdateLinearCoefficientsAction());
            }
        }
    }
}