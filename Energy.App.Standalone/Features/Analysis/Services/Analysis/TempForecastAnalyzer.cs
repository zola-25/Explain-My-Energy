using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.Shared;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastAnalyzer : ITempForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IState<HeatingForecastState> _heatingForecastState;
    private readonly IState<HistoricalForecastState> _historicalForecastState;

    public TempForecastAnalyzer(Co2ConversionFactors co2Conversion,
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
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(term);

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
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(term);

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
        
        List<DailyCostedReading> forecastCosts;

        if (useHistorical)
        {
            forecastCosts = _historicalForecastState.Value[meterType]
                .Where(c => c.UtcTime >= start && c.UtcTime <= end).ToList();
        } else
        {
            forecastCosts = _heatingForecastState.Value.ForecastDailyCosts
                .Where(c => c.UtcTime >= start && c.UtcTime <= end).ToList();
        }


        
        var totalKWh = forecastCosts.Sum(c => c.KWh);
        var totalCost = forecastCosts.Sum(c => c.ReadingTotalCostPounds);
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
                LowDailyTemp = Convert.ToInt32(periodWeatherReadings.Min(c => c.TemperatureCelsius)),
                HighDailyTemp = Convert.ToInt32(periodWeatherReadings.Max(c => c.TemperatureCelsius)),
                AverageTemp = Convert.ToInt32(periodWeatherReadings.Average(c => c.TemperatureCelsius))
            }
        };
        return results;
    }

}