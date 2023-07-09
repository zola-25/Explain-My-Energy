using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using System.Collections.Immutable;
using System.Text.Json;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastAnalyzer : ITempForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IForecastGenerator _forecastGenerator;
    private readonly ICostCalculator _costCalculator;
    IState<WeatherState> _weatherState;
    IState<LinearCoefficientsState> _linearCoefficientsState;

    IState<ElectricityTariffsState> _electricityTariffsState;

    IState<GasTariffsState> _gasTariffsState;


    public TempForecastAnalyzer(Co2ConversionFactors co2Conversion,
        ITermDateRanges periodDateRanges,
        IForecastGenerator forecastGenerator,
        ICostCalculator costCalculator,
        IState<WeatherState> weatherState,
        IState<LinearCoefficientsState> linearCoefficientsState,
        IState<GasTariffsState> gasTariffsState,
        IState<ElectricityTariffsState> electricityTariffsState)
    {
        _co2Conversion = co2Conversion;
        _periodDateRanges = periodDateRanges;
        _forecastGenerator = forecastGenerator;
        _costCalculator = costCalculator;
        _weatherState = weatherState;
        _linearCoefficientsState = linearCoefficientsState;
        _gasTariffsState = gasTariffsState;
        _electricityTariffsState = electricityTariffsState;
    }

    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType,
        CalendarTerm term,
        decimal degreeDifference)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            degreeDifference,
            start,
            end,
            term
        );

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
        CalendarTerm term,
        decimal degreeDifference)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            degreeDifference,
            start,
            end,
            term
        );

        return results;
    }


    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        decimal degreeDifference,
        DateTime start,
        DateTime end,
        CalendarTerm term)
    {
        var periodWeatherReadings = _weatherState.Value.WeatherReadings
            .Where(c => c.UtcReadDate >= start && c.UtcReadDate <= end).
            ToList();

        var forecastBasicReadings = _forecastGenerator.GetBasicReadingsForecast
        (
            degreeDifference,
            _linearCoefficientsState.Value,
            periodWeatherReadings
        );
        
        ImmutableList<TariffDetailState> tariffDetailStates;
        switch (meterType)
        {
            case MeterType.Electricity:
                tariffDetailStates = _electricityTariffsState.Value.TariffDetails;
                break;
            case MeterType.Gas:
                tariffDetailStates = _gasTariffsState.Value.TariffDetails;
                break;
            default:
                throw new NotImplementedException();
        }

        var forecastCosts = _costCalculator.GetCostReadings(forecastBasicReadings, tariffDetailStates);

        var totalKWh = forecastCosts.Sum(c => c.KWh);
        var totalCost = forecastCosts.Sum(c => c.ReadingTotalCostPence) / 100m;
        var totalCo2 = totalKWh * _co2Conversion.GetCo2ConversionFactor(meterType);
        
        var totalKWhRounded = totalKWh.Round(term.NumberOfDecimals());
        var totalCostRounded = totalCost.Round(term.NumberOfDecimals());
        var totalCo2Rounded = totalCo2.Round(1);

        var results = new ForecastAnalysis()
        {
            NumberOfDays = periodWeatherReadings.Count,
            Start = start,
            End = end,
            ForecastConsumption = totalKWhRounded,
            ForecastCostPounds = totalCostRounded,
            ForecastCo2 = totalCo2Rounded,
            TemperatureRange = new TemperatureRange()
            {
                LowDailyTemp = Convert.ToInt32(periodWeatherReadings.Min(c => c.TemperatureAverage)),
                HighDailyTemp = Convert.ToInt32(periodWeatherReadings.Max(c => c.TemperatureAverage)),
                AverageTemp = Convert.ToInt32(periodWeatherReadings.Average(c => c.TemperatureAverage))
            }
        };
        return results;
    }

}