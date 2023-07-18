using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class LoadHeatingForecastAction
{
    public TaskCompletionSource<(bool success, string message)> CompletionSource { get; }
    public decimal DegreeDifference { get; }
    public LoadHeatingForecastAction(decimal degreeDifference = 0, TaskCompletionSource<(bool success, string message)> completionSource = null)
    {
        DegreeDifference = degreeDifference;
        CompletionSource = completionSource;
    }

    [ReducerMethod(typeof(LoadHeatingForecastAction))]
    public static HeatingForecastState LoadHeatingForecastReducer(HeatingForecastState state)
    {
        return state with
        {
            LoadingHeatingForecast = true,
        };
    }


    [ReducerMethod]
    public static HeatingForecastState StoreHeatingForecastReducer(HeatingForecastState state, StoreHeatingForecastAction action)
    {
        return state with
        {
            ForecastDailyCosts = action.ForecastDailyReadings,
            ForecastWeatherReadings = action.TemperaturePoints,
            ForecastsUpdatedWithReadingDate = action.LastUpdatedReadingDate,
            LatestReadingDate = action.LastUpdatedReadingDate,
            LoadingHeatingForecast = false
        };
    }

    private class LoadHeatingForecastEffect : Effect<LoadHeatingForecastAction>
    {
        private readonly IState<HouseholdState> _householdState;
        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<WeatherState> _weatherState;
        private readonly IState<HeatingForecastState> _heatingForecastState;
        private readonly IForecastGenerator _forecastGenerator;
        private readonly ICostCalculator _costCalculator;
        private readonly ICostedReadingsToDailyAggregator _costedReadingsToDailyAggregator;
        private readonly ILogger<LoadHeatingForecastEffect> _logger;

        public LoadHeatingForecastEffect(IState<HouseholdState> householdState,
                                         IState<MeterSetupState> meterSetupState,
                                         IState<WeatherState> weatherState,
                                         IState<HeatingForecastState> heatingForecastState,
                                         IForecastGenerator forecastGenerator,
                                         ICostCalculator costCalculator,
                                         ICostedReadingsToDailyAggregator costedReadingsToDailyAggregator,
                                         ILogger<LoadHeatingForecastEffect> logger)
        {
            _householdState = householdState;
            _meterSetupState = meterSetupState;
            _weatherState = weatherState;
            _heatingForecastState = heatingForecastState;
            _forecastGenerator = forecastGenerator;
            _costCalculator = costCalculator;
            _costedReadingsToDailyAggregator = costedReadingsToDailyAggregator;
            _logger = logger;
        }

        public override async Task HandleAsync(LoadHeatingForecastAction action, IDispatcher dispatcher)
        {
            try
            {
                var heatingMeter = _householdState.Value.PrimaryHeatSource;
                decimal degreeDifference = action.DegreeDifference;

                var tariffs = _meterSetupState.Value[heatingMeter].TariffDetails;
                var heatingForecastState = _heatingForecastState.Value;


                var latestReadingDate = heatingForecastState.LatestReadingDate;

                var recentWeatherReadings = _weatherState.Value.WeatherReadings
                    .Where(c => c.UtcTime >= AppWideForecastProperties.PredictionStartDate(latestReadingDate)
                            && c.UtcTime <= AppWideForecastProperties.PredictionEndDate(latestReadingDate))
                    .ToList();

                var coefficients = new Coefficients { C = heatingForecastState.C, Gradient = heatingForecastState.Gradient };

                var basicReadingsForecast = _forecastGenerator.GetBasicReadingsForecast(degreeDifference,
                                                                                        coefficients,
                                                                                        recentWeatherReadings);

                var costedReadingsForecast = _costCalculator.GetCostReadings(basicReadingsForecast, tariffs);

                var dailyAggregatedCostedReadings = _costedReadingsToDailyAggregator
                                                        .Aggregate(costedReadingsForecast)
                                                        .ToImmutableList();

                var temperaturePoints = recentWeatherReadings.Select
                    (
                        c => new TemperaturePoint()
                        {
                            UtcTime = c.UtcTime,
                            DateTicks = c.UtcTime.eToUnixTicksNoOffset(),
                            TemperatureCelsius = c.TemperatureAverage + degreeDifference,
                            TemperatureCelsiusUnmodified = c.TemperatureAverage,
                            Summary = c.Summary ?? string.Empty
                        }
                    ).ToImmutableList();

                dispatcher.Dispatch(new StoreHeatingForecastAction(dailyAggregatedCostedReadings, temperaturePoints, latestReadingDate));
                dispatcher.Dispatch(new NotifyHeatingForecastUpdatedAction(degreeDifference));
                
                action.CompletionSource?.SetResult((true, string.Empty));
                dispatcher.Dispatch(new NotifyHeatingForecastFinishedAction());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading heating forecast");
                dispatcher.Dispatch(new NotifyHeatingForecastFinishedAction());
            }
        }
    }

}
