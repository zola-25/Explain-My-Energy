using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Fluxor;

namespace Energy.App.Standalone.Features
{
    public class AppInitialization : IAppInitialization
    {
        private readonly IDispatcher _dispatcher;
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;

        public AppInitialization(IState<HouseholdState> householdState,
                                 IState<WeatherState> weatherState,
                                 IState<MeterSetupState> meterSetupState,

                                 IState<GasReadingsState> gasReadingsState,
                                 IState<ElectricityReadingsState> electricityReadingsState,
                                 IDispatcher dispatcher)
        {
            _householdState = householdState;
            _weatherState = weatherState;
            _meterSetupState = meterSetupState;
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
            _dispatcher = dispatcher;
        }

        public void Initialize()
        {
            if (!_householdState.Value.Saved)
            {
                return;
            }

            if (!_weatherState.Value.WeatherReadings.Any())
            {
                _dispatcher.Dispatch(new WeatherLoadReadingsAction(_householdState.Value.OutCodeCharacters));
            }
            else
            {
                DateTime latestReading = _weatherState.Value.WeatherReadings.Where(c => c.IsRecentForecast).OrderBy(c => c.ReadDate).First().ReadDate;
                if (latestReading < DateTime.UtcNow.Date.AddDays(-1))
                {
                    _dispatcher.Dispatch(new WeatherLoadReadingsAction(_householdState.Value.OutCodeCharacters));
                }
            }

            if (_meterSetupState.Value.ElectricityMeter.SetupValid)
            {
                if (_electricityReadingsState.Value.BasicReadings.Any())
                {
                    DateTime lastReading = _electricityReadingsState.Value.BasicReadings.Last().LocalTime;
                    if (lastReading < DateTime.Today.AddDays(-1))
                    {
                        _dispatcher.Dispatch(new ElectricityUpdateReadingsAction(lastReading.Date));
                    }
                }
                else
                {
                    _dispatcher.Dispatch(new ElectricityReloadReadingsAction());
                }
            }

            if (_meterSetupState.Value.GasMeter.SetupValid)
            {
                if (_gasReadingsState.Value.BasicReadings.Any())
                {
                    DateTime lastReading = _gasReadingsState.Value.BasicReadings.Last().LocalTime;
                    if (lastReading < DateTime.Today.AddDays(-1))
                    {
                        _dispatcher.Dispatch(new GasUpdateReadingsAction(lastReading.Date));
                    }
                }
                else
                {
                    _dispatcher.Dispatch(new GasReloadReadingsAction());
                }
            }
        }
    }
}
