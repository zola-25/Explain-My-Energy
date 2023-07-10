using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store.OldAppInit
{
    public class AppInit
    {
        private readonly IDispatcher _dispatcher;
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<HeatingForecastState> _heatingForecastState;
        IActionSubscriber _actionSubscriber;

        public AppInit(IState<HouseholdState> householdState,
            IState<WeatherState> weatherState,
            IState<MeterSetupState> meterSetupState,
            IState<GasReadingsState> gasReadingsState,
            IState<ElectricityReadingsState> electricityReadingsState,
            IDispatcher dispatcher,
            IState<HeatingForecastState> heatingForecastState,
            IActionSubscriber actionSubscriber)
        {
            _householdState = householdState;
            _weatherState = weatherState;
            _meterSetupState = meterSetupState;
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
            _dispatcher = dispatcher;
            _heatingForecastState = heatingForecastState;
            _actionSubscriber = actionSubscriber;
        }


        public bool HouseholdSetupValid => _householdState.Value.Saved;
        public bool ElectricityMeterSetupValid => _meterSetupState.Value.ElectricityMeter.SetupValid;
        public bool GasMeterSetupValid => _meterSetupState.Value.GasMeter.SetupValid;

        public bool WeatherDataLoading => _weatherState.Value.Loading || _weatherState.Value.Updating
            || _weatherState.Value.LastUpdated < DateTime.Today;

        public bool ElectricityDataLoading => _electricityReadingsState.Value.ReloadingReadings
                                              || _electricityReadingsState.Value.UpdatingReadings
                                              || _electricityReadingsState.Value.LastUpdated < DateTime.Today;
        public bool GasDataLoading => _gasReadingsState.Value.ReloadingReadings 
            || _gasReadingsState.Value.UpdatingReadings
            || _gasReadingsState.Value.LastUpdated < DateTime.Today;

        public bool AppStarted { get; private set; }

        public void Initialize()
        {
            AppStarted = true;

            if (!HouseholdSetupValid)
            {
                return;
            }


            InitializeWeather();

            InitializeElectricity();

            InitializeGas();

        }

        public void InitializeWeather()
        {
            if (!HouseholdSetupValid)
            {
                return;
            }

            if (!_weatherState.Value.WeatherReadings.Any())
            {
                _dispatcher.Dispatch(new InitiateWeatherReloadReadingsAction(_householdState.Value.OutCodeCharacters));
                return;
            }

            var latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.
                UtcTime;
            var latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.
                UtcTime;

            if (latestReading < DateTime.UtcNow.Date.AddDays(-1))
            {
                _dispatcher.Dispatch
                (
                    new InitiateWeatherUpdateReadingsAction
                    (
                        _householdState.Value.OutCodeCharacters,
                        latestReading,
                        latestHistoricalReading
                    )
                );
                return;
            }
        }

        public void InitializeGas()
        {
            if (!GasMeterSetupValid)
            {
                return;
            }
            if (!_gasReadingsState.Value.CostedReadings.Any())
            {
                _dispatcher.Dispatch(new GasReloadReadingsAndCostsAction());
            }
            else
            {
                DateTime lastReading = _gasReadingsState.Value.CostedReadings.Last().
                    UtcTime;
                if (lastReading < DateTime.Today.AddDays(-1))
                {
                    _dispatcher.Dispatch(new GasUpdateReadingsAndCostsAction(lastReading.Date));
                }
                else  {
                    _dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(0,MeterType.Gas));
                } 


            }

        }

        public void InitializeElectricity()
        {
            if (!ElectricityMeterSetupValid)
            {
                return;
            }

            if (!_electricityReadingsState.Value.CostedReadings.Any())
            {
                _dispatcher.Dispatch(new ElectricityReloadReadingsAndCostsAction());
            }
            else
            {
                DateTime lastReading = _electricityReadingsState.Value.CostedReadings.Last().
                    UtcTime;
                if (lastReading < DateTime.Today.AddDays(-1))
                {
                    _dispatcher.Dispatch(new ElectricityUpdateReadingsAndCostsAction(lastReading.Date));
                }
                else {
                    _dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(0,MeterType.Gas));
                } 
            }

        }

    }
}