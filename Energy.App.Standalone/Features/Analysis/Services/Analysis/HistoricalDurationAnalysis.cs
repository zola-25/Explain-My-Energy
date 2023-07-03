using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;
using MathNet.Numerics;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class HistoricalDurationAnalyzer : IHistoricalDurationAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    readonly ITermDateRanges _periodDateRanges;



    public HistoricalDurationAnalyzer(ITermDateRanges periodDateRanges)
    {
        _periodDateRanges = periodDateRanges;
        _co2Conversion = new Co2ConversionFactors();
    }

    public HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType, 
        CalendarTerm duration,
        ImmutableList<CostedReading> costedReadings, 
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);

        return GetHistoricalAnalysis(meterType, start, end, costedReadings, dailyWeatherReadings);
    }

    public HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType, 
        CalendarTerm duration,
        ImmutableList<CostedReading> costedReadings, 
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetPreviousPeriodDates(duration);

        return GetHistoricalAnalysis(meterType, start, end, costedReadings, dailyWeatherReadings);
    }

    private HistoricalAnalysis GetHistoricalAnalysis(MeterType meterType, 
        DateTime start, DateTime end,
        ImmutableList<CostedReading> costedReadings, 
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        var weatherReadings = dailyWeatherReadings.Where(c => c.UtcReadDate >= start && c.UtcReadDate < end)
            .ToList();

        var durationData = costedReadings.FindAll(c => c.LocalTime >= start && c.LocalTime < end);

        decimal co2ConversionFactor = _co2Conversion.GetCo2ConversionFactor(meterType);

        bool hasData = durationData.Count > 0;

        if (hasData)
        {
            return new HistoricalAnalysis()
            {
                HasData = true,
                Start = start,
                End = end.AddDays(-1),
                LatestReading = durationData.Last().LocalTime.Date,
                PeriodCo2 = (durationData.Sum(c => c.KWh) * co2ConversionFactor).Round(1),
                PeriodConsumptionKWh = durationData.Sum(c => c.KWh).Round(0),
                PeriodCost = durationData.Sum(c => c.CostPence).Round(2),
                TemperatureRange = weatherReadings.Any() ? new TemperatureRange()
                {
                    LowDailyTemp = Convert.ToInt32(weatherReadings.Min(c => c.TemperatureAverage)),
                    HighDailyTemp = Convert.ToInt32(weatherReadings.Max(c => c.TemperatureAverage)),
                    AverageTemp = Convert.ToInt32(weatherReadings.Average(c => c.TemperatureAverage))
                } : new TemperatureRange()
            };
        }

        return new HistoricalAnalysis()
        {
            HasData = false,
            Start = start,
            End = end.AddDays(-1),
            TemperatureRange = new TemperatureRange()
        };
    }

}