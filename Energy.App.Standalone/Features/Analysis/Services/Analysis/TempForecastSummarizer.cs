using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.Shared;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastSummarizer : ITempForecastSummarizer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IState<HeatingForecastState> _heatingForecastState;
    private readonly IState<HistoricalForecastState> _historicalForecastState;

    public TempForecastSummarizer(Co2ConversionFactors co2Conversion,
        ITermDateRanges periodDateRanges,
        IState<HeatingForecastState> heatingForecastState,
        IState<HistoricalForecastState> historicalForecastState)
    {
        _co2Conversion = co2Conversion;
        _periodDateRanges = periodDateRanges;
        _heatingForecastState = heatingForecastState;
        _historicalForecastState = historicalForecastState;
    }

    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType,
        CalendarTerm term, bool useHistorical)
    {
        (var start, var end) = _periodDateRanges.GetNextPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            start,
            end,
            term,
            useHistorical
        );

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
        CalendarTerm term, bool useHistorical)
    {
        (var start, var end) = _periodDateRanges.GetCurrentPeriodDates(term);

        var results = ForecastAnalysis
        (
            meterType,
            start,
            end,
            term,
            useHistorical
        );

        return results;
    }


    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        DateTime start,
        DateTime end,
        CalendarTerm term,
        bool useHistorical)
    {
        var periodWeatherReadings = _heatingForecastState.Value.ForecastWeatherReadings
                                        .Where(c => c.UtcTime >= start && c.UtcTime <= end)
                                        .ToList();

        var forecastCosts = useHistorical
            ? _historicalForecastState.Value[meterType]
                .Where(c => c.UtcTime >= start && c.UtcTime <= end).ToList()
            : _heatingForecastState.Value.ForecastDailyCosts
                .Where(c => c.UtcTime >= start && c.UtcTime <= end).ToList();



        decimal totalKWh = forecastCosts.Sum(c => c.KWh);
        decimal totalCost = forecastCosts.Sum(c => c.ReadingTotalCostPounds);
        decimal totalCo2 = totalKWh * _co2Conversion.GetCo2ConversionFactor(meterType);

        decimal totalKWhRounded = totalKWh.Round(term.NumberOfDecimals());
        decimal totalCostRounded = totalCost.Round(term.NumberOfDecimals());
        decimal totalCo2Rounded = totalCo2.Round(1);

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
                LowDailyTemp = Convert.ToInt32(periodWeatherReadings.Min(c => c.TemperatureCelsius)),
                HighDailyTemp = Convert.ToInt32(periodWeatherReadings.Max(c => c.TemperatureCelsius)),
                AverageTemp = Convert.ToInt32(periodWeatherReadings.Average(c => c.TemperatureCelsius))
            }
        };
        return results;
    }
}