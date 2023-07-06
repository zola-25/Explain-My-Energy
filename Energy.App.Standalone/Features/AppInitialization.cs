using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

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
            bool canUpdateWeatherData = _householdState.Value.Saved;
            bool canUpdateElectricityData = _meterSetupState.Value.ElectricityMeter.SetupValid;
            bool canUpdateGasData = _meterSetupState.Value.ElectricityMeter.SetupValid;
            bool canUpdateLinearCoefficients =  _meterSetupState.Value[_householdState.Value.PrimaryHeatSource].SetupValid;

            _dispatcher.Dispatch(new AppInitializationUpdateWeatherDataLoadableAction(canUpdateWeatherData));
            _dispatcher.Dispatch(new AppInitializationUpdateElectricityReadingsDataLoadableAction(canUpdateElectricityData));
            _dispatcher.Dispatch(new AppInitializationUpdateGasReadingsDataLoadableAction(canUpdateGasData));
            _dispatcher.Dispatch(new AppInitializationUpdateLinearCoefficientsLoadableAction(canUpdateLinearCoefficients));

            if (!_householdState.Value.Saved)
            {
                return;
            }
            
            
            InitializeWeather();
            
            if (_meterSetupState.Value.ElectricityMeter.SetupValid)
            {
                if (_electricityReadingsState.Value.BasicReadings.Any())
                {
                    DateTime lastReading = _electricityReadingsState.Value.BasicReadings.Last().UtcTime;
                    if (lastReading < DateTime.Today.AddDays(-1))
                    {
                        _dispatcher.Dispatch(new ElectricityUpdateReadingsAction(lastReading.Date));
                    }
                }
                else
                {
                    _dispatcher.Dispatch(new ElectricityReloadReadingsAction());
                }

                _dispatcher.Dispatch(new ElectricityInitiateCostCalculationsAction());


                if (_householdState.Value.PrimaryHeatSource == MeterType.Electricity)
                {
                    _dispatcher.Dispatch(new InitiateUpdateLinearCoeffiecientsAction());

                }
            }

            if (_meterSetupState.Value.GasMeter.SetupValid)
            {
                if (_gasReadingsState.Value.BasicReadings.Any())
                {
                    DateTime lastReading = _gasReadingsState.Value.BasicReadings.Last().UtcTime;
                    if (lastReading < DateTime.Today.AddDays(-1))
                    {
                        _dispatcher.Dispatch(new GasUpdateReadingsAction(lastReading.Date));
                    }
                }
                else
                {
                    _dispatcher.Dispatch(new GasReloadReadingsAction());
                }

                _dispatcher.Dispatch(new GasInitiateCostCalculationsAction());

                if (_householdState.Value.PrimaryHeatSource == MeterType.Gas)
                {
                    _dispatcher.Dispatch(new InitiateUpdateLinearCoeffiecientsAction());
                }
            }

            void InitializeWeather()
            {
                if (!_weatherState.Value.WeatherReadings.Any())
                {
                    _dispatcher.Dispatch(new WeatherReloadReadingsAction(_householdState.Value.OutCodeCharacters));
                }
                else
                {
                    var latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.UtcReadDate;
                    var latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.UtcReadDate;

                    if (latestReading < DateTime.UtcNow.Date.AddDays(-1))
                    {
                        _dispatcher.Dispatch(new WeatherUpdateReadingsAction(_householdState.Value.OutCodeCharacters,
                                                                             latestReading,
                                                                             latestHistoricalReading));
                    } 
                    else
                    {
                        _dispatcher.Dispatch(new NotifyWeatherReadingsLoadedAction());
                    }
                    
                }
            }
        }

        
    }
}
