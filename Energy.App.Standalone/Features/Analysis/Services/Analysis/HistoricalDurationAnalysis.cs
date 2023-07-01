using Energy.App.Blazor.Client.StateContainers;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class HistoricalDurationAnalyzer : IHistoricalDurationAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    readonly ITermDateRanges _periodDateRanges;
    private readonly MeterDataState _meterDataState;
    private readonly WeatherDataState _weatherDataState;


    public HistoricalDurationAnalyzer(ITermDateRanges periodDateRanges, MeterDataState meterDataState, WeatherDataState weatherDataState)
    {
        _periodDateRanges = periodDateRanges;
        _meterDataState = meterDataState;
        _weatherDataState = weatherDataState;
        _co2Conversion = new Co2ConversionFactors();
    }

    public HistoricalAnalysis GetCurrentDurationAnalysis(List<BasicReading> readings, List<DailyWeatherReading> dailyWeatherReadings, CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);

        return GetHistoricalAnalysis(readings, dailyWeatherReadings, start, end);
    }

    public HistoricalAnalysis GetPreviousDurationAnalysis(List<BasicReading> readings, List<DailyWeatherReading> dailyWeatherReadings, CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetPreviousPeriodDates(duration);

        return GetHistoricalAnalysis(readings, dailyWeatherReadings, start, end);
    }

    private HistoricalAnalysis GetHistoricalAnalysis(List<BasicReading> readings, List<DailyWeatherReading> dailyWeatherReadings, DateTime start, DateTime end)
    {
        var costedReadings = _meterDataState.GetCostedReadings(meterType);
        var weatherReadings = dailyWeatherReadings.Where(c => c.ReadDate >= start && c.ReadDate < end)
            .ToList();

        var durationData = costedReadings
            .Where(c => c.LocalTime >= start && c.LocalTime < end).ToList();

        double co2ConversionFactor = _co2Conversion.GetCo2ConversionFactor(meterType);

        bool hasData = durationData.Any();

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
                    LowDailyTemp = Convert.ToInt32(weatherReadings.Min(c => c.TemperatureMeanHourly)),
                    HighDailyTemp = Convert.ToInt32(weatherReadings.Max(c => c.TemperatureMeanHourly)),
                    AverageTemp = Convert.ToInt32(weatherReadings.Average(c => c.TemperatureMeanHourly))
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