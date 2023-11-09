using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using Energy.Shared;
using Fluxor;
using MathNet.Numerics;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class HistoricalForecastSummarizer : IHistoricalForecastSummarizer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IState<HistoricalForecastState> _historicalForecastState;

    public HistoricalForecastSummarizer(Co2ConversionFactors co2Conversion,
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
        (var start, var end) = _periodDateRanges.GetNextPeriodDates(term);

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
        (var start, var end) = _periodDateRanges.GetCurrentPeriodDates(term);

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

        decimal totalKWh = forecastDailyCosts.Sum(c => c.KWh);
        decimal totalCost = forecastDailyCosts.Sum(c => c.ReadingTotalCostPounds);
        decimal totalCo2 = totalKWh * _co2Conversion.GetCo2ConversionFactor(meterType);

        decimal totalKWhRounded = totalKWh.Round(term.NumberOfDecimals());
        decimal totalCostRounded = totalCost.Round(term.NumberOfDecimals());
        decimal totalCo2Rounded = totalCo2.Round(1);

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