using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Store;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Setup.Store.MeterSetup;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class AppStateValidator : IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;
        private readonly IState<ElectricityReadingsState> _electricityReadingsState;
        private readonly IState<HeatingForecastState> _heatingForecastState;
        IActionSubscriber _actionSubscriber;


        public AppStateValidator(IState<HouseholdState> householdState,
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


        private bool HouseholdSetupValid => _householdState.Value.Saved;
        private bool ElectricityMeterSetupValid => _meterSetupState.Value.ElectricityMeter.SetupValid;
        private bool GasMeterSetupValid => _meterSetupState.Value.GasMeter.SetupValid;

        public void Validate(TaskCompletionSource<bool> taskCompletion)
        {
            _dispatcher.Dispatch(new InitializeAppAction());

            if (!HouseholdSetupValid)
            {
                return;
            }


            InitializeWeather();
            _actionSubscriber.SubscribeToAction<NotifyElectricityStoreReady>(this, action =>
            {
                if (_householdState.Value.PrimaryHeatSource == MeterType.Electricity)
                {
                    _dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction(action.NumberOfUpdatedReadings, MeterType.Electricity));
                }
            });

            InitializeElectricity();


            _actionSubscriber.SubscribeToAction<NotifyGasStoreReady>(this, action =>
            {
                if (_householdState.Value.PrimaryHeatSource == MeterType.Gas)
                {
                    _dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction(action.NumberOfUpdatedReadings, MeterType.Electricity));
                }
            });

            InitializeGas();

            taskCompletion.SetResult(true);
        }

        public void InitializeWeather()
        {

        }

        public void InitializeGas()
        {



            if (!GasMeterSetupValid)
            {
                return;
            }

            System.Collections.Immutable.ImmutableList<BasicReading> existingBasicReadings = _gasReadingsState.Value.BasicReadings;
            if (existingBasicReadings.eIsNullOrEmpty())
            {
                _dispatcher.Dispatch(new GasReloadReadingsAndCostsAction());
            }
            else
            {

                DateTime lastBasicReading = _gasReadingsState.Value.BasicReadings.Last().
                    UtcTime;

                if (lastBasicReading < DateTime.Today.AddDays(-1))
                {
                    _dispatcher.Dispatch(new GasUpdateReadingsAndReloadCostsAction(lastBasicReading.Date));
                }
                else
                {
                    System.Collections.Immutable.ImmutableList<Analysis.Services.DataLoading.Models.CostedReading> costedReadings = _gasReadingsState.Value.CostedReadings;
                    DateTime lastCostedReading = costedReadings.Last().UtcTime;

                    if (costedReadings.eIsNullOrEmpty()
                        || lastCostedReading <= lastBasicReading)
                    {
                        _dispatcher.Dispatch(new GasReloadCostsOnlyAction());
                    }
                    else
                    {
                        _dispatcher.Dispatch(new NotifyGasStoreReady(0, 0));

                    }
                }

            }

        }

        public void InitializeElectricity()
        {


            if (!ElectricityMeterSetupValid)
            {
                return;
            }

            System.Collections.Immutable.ImmutableList<BasicReading> existingBasicReadings = _electricityReadingsState.Value.BasicReadings;
            if (existingBasicReadings.eIsNullOrEmpty())
            {
                _dispatcher.Dispatch(new ElectricityReloadReadingsAndCostsAction());
            }
            else
            {

                DateTime lastBasicReading = _electricityReadingsState.Value.BasicReadings.Last().
                    UtcTime;

                if (lastBasicReading < DateTime.Today.AddDays(-1))
                {
                    _dispatcher.Dispatch(new ElectricityUpdateReadingsAndReloadCostsAction(lastBasicReading.Date));
                }
                else
                {
                    System.Collections.Immutable.ImmutableList<Analysis.Services.DataLoading.Models.CostedReading> costedReadings = _electricityReadingsState.Value.CostedReadings;
                    DateTime lastCostedReading = costedReadings.Last().UtcTime;

                    if (costedReadings.eIsNullOrEmpty()
                        || lastCostedReading <= lastBasicReading)
                    {
                        _dispatcher.Dispatch(new ElectricityReloadCostsOnlyAction());
                    }
                    else
                    {
                        _dispatcher.Dispatch(new NotifyElectricityStoreReady(0, 0));

                    }

                }

            }

        }

        public void Dispose()
        {
            _actionSubscriber.UnsubscribeFromAllActions(this);
        }
    }
}