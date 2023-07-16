using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using Energy.Shared;
using Fluxor;
using MathNet.Numerics;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class SimpleForecastAnalyzer : ISimpleForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IState<HistoricalForecastState> _historicalForecastState;

    public SimpleForecastAnalyzer(Co2ConversionFactors co2Conversion,
                                  ITermDateRanges periodDateRanges,
                                  IState<HistoricalForecastState> historicalForecastState)
    {
        _co2Conversion = co2Conversion;
        _periodDateRanges = periodDateRanges;
        _historicalForecastState = historicalForecastState;
    }

    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType,
        CalendarTerm term)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            start,
            end,
            term
        );

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
        CalendarTerm term)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            start,
            end,
            term
        );

        return results;
    }


    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        DateTime start,
        DateTime end,
        CalendarTerm term)
    {

        var forecastDailyCosts = _historicalForecastState.Value[meterType]
            .Where(c => c.UtcTime >= start && c.UtcTime <= end).ToList();

        var totalKWh = forecastDailyCosts.Sum(c => c.KWh);
        var totalCost = forecastDailyCosts.Sum(c => c.ReadingTotalCostPounds);
        var totalCo2 = totalKWh * _co2Conversion.GetCo2ConversionFactor(meterType);
        
        var totalKWhRounded = totalKWh.Round(term.NumberOfDecimals());
        var totalCostRounded = totalCost.Round(term.NumberOfDecimals());
        var totalCo2Rounded = totalCo2.Round(1);

        var results = new ForecastAnalysis()
        {
            NumberOfDays = forecastDailyCosts.Count,
            Start = start,
            End = end,
            ForecastConsumption = totalKWhRounded,
            ForecastCostPounds = totalCostRounded,
            ForecastCo2 = totalCo2Rounded,
            TemperatureRange = new TemperatureRange()
        };
        return results;
    }

}