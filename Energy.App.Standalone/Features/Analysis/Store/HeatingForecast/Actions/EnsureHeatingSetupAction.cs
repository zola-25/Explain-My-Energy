using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;
using System.ComponentModel.DataAnnotations;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class EnsureHeatingSetupAction
    {
        public TaskCompletionSource<(bool success, string message)> Completion { get; }

        public bool ForceReloadCoefficients { get; }

        public bool ForceReloadHeatingForecast { get; }
        
        public EnsureHeatingSetupAction(bool forceReloadHeatingForecast, bool forceReloadCoefficients, TaskCompletionSource<(bool success, string message)> completion = null)
        {
            Completion = completion;
            ForceReloadCoefficients = forceReloadCoefficients;
            ForceReloadHeatingForecast = forceReloadHeatingForecast;
        }

        [ReducerMethod(typeof(NotifyHeatingSetupFinishedAction))]
        public static HeatingForecastState NotifyHeatingSetupFinishedReducer(HeatingForecastState state)
        {
            return state with
            {
                LoadingCoefficients = false
            };
        }

        [ReducerMethod(typeof(EnsureHeatingSetupAction))]
        public static HeatingForecastState EnsureHeatingSetupReducer(HeatingForecastState state)
        {
            return state with
            {
                LoadingCoefficients = true
            };
        }

        [ReducerMethod]
        public static HeatingForecastState StoreCoefficientsReducer(HeatingForecastState state, StoreCoefficientsAction action)
        {
            return state with
            {
                SavedCoefficients = true,
                Gradient = action.Gradient,
                C = action.C,
                HeatingMeterType = action.HeatingMeterType,
                CoefficientsUpdatedWithReadingDate = action.LastUpdatedReadingDate,
                LatestReadingDate = action.LastUpdatedReadingDate
            };
        }

        private class EnsureHeatingSetupActionEffect : Effect<EnsureHeatingSetupAction>
        {
            private readonly IState<HouseholdState> _householdState;
            private readonly IState<WeatherState> _weatherState;
            private readonly IState<HeatingForecastState> _heatingForecastState;
            private readonly IState<AnalysisOptionsState> _analysisOptionsState;
            private readonly IState<MeterSetupState> _meterSetupState;
            private readonly IState<HeatingForecastState> _state;
            private readonly IState<GasReadingsState> _gasReadingsState;
            private readonly IState<ElectricityReadingsState> _electricityReadingsState;
            private readonly IForecastCoefficientsCreator _forecastCoefficientsCreator;
            private readonly ILogger<EnsureHeatingSetupActionEffect> _logger;

            public EnsureHeatingSetupActionEffect(IState<HouseholdState> householdState,
                                                  IState<WeatherState> weatherState,
                                                  IState<HeatingForecastState> heatingForecastState,
                                                  IState<AnalysisOptionsState> analysisOptionsState,
                                                  IState<MeterSetupState> meterSetupState,
                                                  IState<HeatingForecastState> state,
                                                  IState<GasReadingsState> gasReadingsState,
                                                  IState<ElectricityReadingsState> electricityReadingsState,
                                                  IForecastCoefficientsCreator forecastCoefficientsCreator,
                                                  ILogger<EnsureHeatingSetupActionEffect> logger)
            {
                _householdState = householdState;
                _weatherState = weatherState;
                _heatingForecastState = heatingForecastState;
                _analysisOptionsState = analysisOptionsState;
                _meterSetupState = meterSetupState;
                _state = state;
                _gasReadingsState = gasReadingsState;
                _electricityReadingsState = electricityReadingsState;
                _forecastCoefficientsCreator = forecastCoefficientsCreator;
                _logger = logger;
            }

            public override async Task HandleAsync(EnsureHeatingSetupAction action, IDispatcher dispatcher)
            {
                var heatingMeterType = _householdState.Value.PrimaryHeatSource;
                var meterSetup = _meterSetupState.Value[heatingMeterType];
                if (!meterSetup.SetupValid)
                {
                    string meterNotSetupMessage = $"Heating Forecast Setup: Cannot load heating forecast for {heatingMeterType} Meter as it is not setup";
                    dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(false, meterNotSetupMessage));
                    action.Completion?.SetResult((false, meterNotSetupMessage));
                    return;
                }

                if (meterSetup.TariffDetails.eIsNullOrEmpty())
                {
                    string noTariffsMessage = $"Heating Forecast Setup: Cannot create heating forecast for {heatingMeterType} Meter as tariff details are not setup";
                    dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(false, noTariffsMessage));
                    action.Completion?.SetResult((false, noTariffsMessage));
                    return;
                }

                

                var basicReadings = heatingMeterType switch
                {
                    MeterType.Gas => _gasReadingsState.Value.BasicReadings,
                    MeterType.Electricity => _electricityReadingsState.Value.BasicReadings,
                    _ => throw new NotImplementedException()
                };

                var latestReadingDate = basicReadings.Last().UtcTime;

                var weatherReadings = _weatherState.Value.WeatherReadings;
                var numLowTempDays = AppWideForecastProperties.GetLowTemperatureDays(DateTime.UtcNow);
                int forgiveMissingDays = 10;

                if (weatherReadings.eIsNullOrEmpty()
                    || weatherReadings.Count(c => c.UtcTime <= DateTime.UtcNow && AppWideForecastProperties.LowTemperatureMonths.Contains(c.UtcTime.Month)) < numLowTempDays - forgiveMissingDays)
                {
                    string noSeasonalWeatherMessage = @$"Heating Forecast Setup: Cannot create heating forecast for {heatingMeterType} Meter 
                                        as there are many missing cold season historical weather readings";

                    dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(false, noSeasonalWeatherMessage));

                    action.Completion?.SetResult((false, noSeasonalWeatherMessage));
                    return;
                }

                if (basicReadings.eIsNullOrEmpty())
                {
                    string noReadingsMessage = @$"Heating Forecast Setup: Cannot create heating forecast for {heatingMeterType} Meter 
                                        as there are no {heatingMeterType} readings";

                    dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(false, noReadingsMessage));

                    action.Completion?.SetResult((false, noReadingsMessage));
                    return;
                }

                if(basicReadings.Count(c => AppWideForecastProperties.LowTemperatureMonths.Contains(c.UtcTime.Month)) < (numLowTempDays - forgiveMissingDays) * 48)
                {
                    string noSeasonalEnergyReadingsMessage = @$"Heating Forecast Setup: Cannot create heating forecast for {heatingMeterType} Meter 
                                        as there are many missing cold season {heatingMeterType} readings";

                    dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(false, noSeasonalEnergyReadingsMessage));

                    action.Completion?.SetResult((false, noSeasonalEnergyReadingsMessage));
                    return;
                }


                var heatingForecastState = _heatingForecastState.Value;
                var timeElapsedSinceLatestCoeffReading = latestReadingDate - heatingForecastState.CoefficientsUpdatedWithReadingDate;

                bool reloadedCoefficients = false;

                if (!heatingForecastState.SavedCoefficients
                    || heatingForecastState.HeatingMeterType != heatingMeterType
                    || timeElapsedSinceLatestCoeffReading.TotalDays >= 7

                    || action.ForceReloadCoefficients)
                {
                    var (C, Gradient) = _forecastCoefficientsCreator.GetForecastCoefficients(basicReadings, weatherReadings);
                    
                    dispatcher.Dispatch(new StoreCoefficientsAction(C, Gradient, heatingMeterType, latestReadingDate));
                    
                    reloadedCoefficients = true;
                }

                decimal degreeDifference = _analysisOptionsState?.Value?[heatingMeterType]?.DegreeDifference ?? 0;
                    
                var loadHeatingCompletion = new TaskCompletionSource<(bool success, string message)>();

                if (reloadedCoefficients 
                    || action.ForceReloadHeatingForecast
                    || heatingForecastState.ForecastWeatherReadings.eIsNullOrEmpty()
                    || heatingForecastState.ForecastDailyCosts.eIsNullOrEmpty()
                    || heatingForecastState.ForecastsUpdatedWithReadingDate < DateTime.UtcNow.Date.AddDays(-1))
                {
                    dispatcher.Dispatch(new LoadHeatingForecastAction(degreeDifference, loadHeatingCompletion));
                }
                else
                {
                    string heatingForecastAlreadyLoadedMessage = 
                        $"Heating Forecast: Using cached heating forecast for {heatingMeterType} Meter";
                    loadHeatingCompletion.SetResult((true, heatingForecastAlreadyLoadedMessage));
                }
                var (loadingSuccess, loadingResultMessage) = await loadHeatingCompletion.Task;
                
                string heatingForecastSetupMessage = reloadedCoefficients ? $"Heating Forecast Setup: new Temperature-Consumption relationship analysed"
                        : $"Heating Forecast Setup: Using cached Temperature-Consumption relationship";
                
                string finalResultMessage = $"{heatingForecastSetupMessage}{Environment.NewLine}{loadingResultMessage}";
                
                dispatcher.Dispatch(new NotifyHeatingSetupFinishedAction(loadingSuccess, finalResultMessage));

                action.Completion?.SetResult((loadingSuccess, finalResultMessage));
            }
        }
    }















}
