using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Weather.Store;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class HistoricalDurationAnalyzer : IHistoricalDurationAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    readonly ITermDateRanges _periodDateRanges;
    IState<ElectricityReadingsState> _electricityReadingsState;
    IState<GasReadingsState> _gasReadingsState;
    IState<WeatherState> _weatherState;


    public HistoricalDurationAnalyzer(ITermDateRanges periodDateRanges, IState<GasReadingsState> gasReadingsState, IState<ElectricityReadingsState> electricityReadingsState, IState<WeatherState> weatherState)
    {
        _periodDateRanges = periodDateRanges;
        _co2Conversion = new Co2ConversionFactors();
        _gasReadingsState = gasReadingsState;
        _electricityReadingsState = electricityReadingsState;
        _weatherState = weatherState;
    }

    public HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType, 
        CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);

        return GetHistoricalAnalysis(meterType, start, end);
    }

    public HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType, 
        CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetPreviousPeriodDates(duration);

        return GetHistoricalAnalysis(meterType, start, end);
    }

    private HistoricalAnalysis GetHistoricalAnalysis(MeterType meterType, 
        DateTime start, DateTime end)
    {
        var costedReadings = meterType == MeterType.Electricity ? _electricityReadingsState.Value.CostedReadings : _gasReadingsState.Value.CostedReadings;
        var weatherReadings = _weatherState.Value.WeatherReadings.Where(c => c.UtcReadDate >= start && c.UtcReadDate <= end)
            .ToList();

        var durationData = costedReadings.FindAll(c => c.UtcTime >= start && c.UtcTime <= end);

        decimal co2ConversionFactor = _co2Conversion.GetCo2ConversionFactor(meterType);

        bool hasData = durationData.Count > 0;

        if (hasData)
        {
            return new HistoricalAnalysis()
            {
                HasData = true,
                Start = start,
                End = end.AddDays(-1),
                LatestReading = durationData.Last().UtcTime.Date,
                PeriodCo2 = (durationData.Sum(c => c.KWh) * co2ConversionFactor).Round(1),
                PeriodConsumptionKWh = durationData.Sum(c => c.KWh).Round(0),
                PeriodCostPounds = (durationData.Sum(c => c.ReadingTotalCostPence) / 100m).Round(2),
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