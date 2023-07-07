﻿using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store.OldAppInit
{
    public class AppInit : IAppInit
    {
        private readonly IDispatcher _dispatcher;
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<LinearCoefficientsState> _linearCoefficientsState;

        public AppInit(IState<HouseholdState> householdState,
            IState<WeatherState> weatherState,
            IState<MeterSetupState> meterSetupState,
            IState<GasReadingsState> gasReadingsState,
            IState<ElectricityReadingsState> electricityReadingsState,
            IDispatcher dispatcher,
            IState<LinearCoefficientsState> linearCoefficientsState)
        {
            _householdState = householdState;
            _weatherState = weatherState;
            _meterSetupState = meterSetupState;
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
            _dispatcher = dispatcher;
            _linearCoefficientsState = linearCoefficientsState;
        }


        public bool CanUpdateWeatherData => _householdState.Value.Saved;
        public bool CanUpdateElectricityData => _meterSetupState.Value.ElectricityMeter.SetupValid;
        public bool CanUpdateGasData => _meterSetupState.Value.GasMeter.SetupValid;

        public bool CanUpdateLinearCoefficients =>
            _meterSetupState.Value[_householdState.Value.PrimaryHeatSource].
                SetupValid;

        public bool WeatherDataLoading => _weatherState.Value.Loading || _weatherState.Value.Updating;
        public bool ElectricityDataLoading => _electricityReadingsState.Value.ReloadingReadings || _electricityReadingsState.Value.UpdatingReadings || ElectricityReadingsState.Value.CalculatingCosts;
        public bool GasDataLoading => _gasReadingsState.Value.Reloading || _gasReadingsState.Value.Updating || _gasReadingsState.Value.CalculatingCosts;

        public bool LinearCoefficientsLoading => !_linearCoefficientsState.Value.Saved;


        public void Initialize()
        {
            if (!_householdState.Value.Saved)
            {
                return;
            }

            if (CanUpdateWeatherData)
            {
                InitializeWeather();
            }

            if (CanUpdateElectricityData)
            {
                InitializeElectricity();
            }

            if (CanUpdateGasData)
            {
                InitializeGas();
            }

            if (CanUpdateLinearCoefficients)
            {
                InitializeLinearCoefficients();
            }
        }

        private void InitializeWeather()
        {
            if (!_weatherState.Value.WeatherReadings.Any())
            {
                _dispatcher.Dispatch(new InitiateWeatherReloadReadingsAction(_householdState.Value.OutCodeCharacters));
                return;
            }

            var latestReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsRecentForecast)?.
                UtcReadDate;
            var latestHistoricalReading = _weatherState.Value.WeatherReadings.FindLast(c => c.IsHistorical)?.
                UtcReadDate;

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

        private void InitializeGas()
        {
            if (_gasReadingsState.Value.BasicReadings.Any())
            {
                DateTime lastReading = _gasReadingsState.Value.BasicReadings.Last().
                    UtcTime;
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
        }

        private void InitializeElectricity()
        {
            if (_electricityReadingsState.Value.BasicReadings.Any())
            {
                DateTime lastReading = _electricityReadingsState.Value.BasicReadings.Last().
                    UtcTime;
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
        }

        private void InitializeLinearCoefficients()
        {
            _dispatcher.Dispatch(new InitiateUpdateLinearCoefficientsAction());
        }
    }
}