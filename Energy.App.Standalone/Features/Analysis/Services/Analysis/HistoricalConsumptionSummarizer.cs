using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.Shared;
using MathNet.Numerics;
using Fluxor;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class HistoricalConsumptionSummarizer : IHistoricalConsumptionSummarizer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IState<ElectricityReadingsState> _electricityReadingsState;
    private readonly IState<GasReadingsState> _gasReadingsState;
    private readonly IState<WeatherState> _weatherState;


    public HistoricalConsumptionSummarizer(ITermDateRanges periodDateRanges, IState<GasReadingsState> gasReadingsState, IState<ElectricityReadingsState> electricityReadingsState, IState<WeatherState> weatherState)
    {
        _periodDateRanges = periodDateRanges;
        _co2Conversion = new Co2ConversionFactors();
        _gasReadingsState = gasReadingsState;
        _electricityReadingsState = electricityReadingsState;
        _weatherState = weatherState;
    }

    public HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType,
        CalendarTerm term)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(term);

        return GetHistoricalAnalysis(meterType, start, end, term);
    }

    public HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType,
        CalendarTerm term)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetPreviousPeriodDates(term);

        return GetHistoricalAnalysis(meterType, start, end, term);
    }

    private HistoricalAnalysis GetHistoricalAnalysis(MeterType meterType,
        DateTime start, DateTime end, CalendarTerm term)
    {
        var costedReadings = (meterType == MeterType.Electricity
            ? _electricityReadingsState.Value.CostedReadings : _gasReadingsState.Value.CostedReadings).ToList();
        var weatherReadings = _weatherState.Value.WReadings.Where(c => c.Utc >= start && c.Utc <= end)
            .ToList();

        var historicalCosts = costedReadings.FindAll(c => c.UtcTime >= start && c.UtcTime < end.AddDays(1));

        decimal co2ConversionFactor = _co2Conversion.GetCo2ConversionFactor(meterType);

        bool hasData = historicalCosts.Count > 0;

        var totalKWh = historicalCosts.Sum(c => c.KWh);
        var totalCost = historicalCosts.Sum(c => c.CostPounds);
        var totalCo2 = totalKWh * co2ConversionFactor;

        var totalKWhRounded = totalKWh.Round(term.NumberOfDecimals());
        var totalCostRounded = totalCost.Round(term.NumberOfDecimals());
        var totalCo2Rounded = totalCo2.Round(2);

        if (hasData)
        {
            return new HistoricalAnalysis()
            {
                HasData = true,
                Start = start,
                End = end,
                LatestReading = historicalCosts.Last().UtcTime.Date,
                PeriodCo2 = totalCo2Rounded,
                PeriodConsumptionKWh = totalKWhRounded,
                PeriodCostPounds = totalCostRounded,
                TemperatureRange = weatherReadings.Any() ? new TemperatureRange()
                {
                    LowDailyTemp = weatherReadings.Min(c => c.TempAvg).Round(0).ToInt(),
                    HighDailyTemp = weatherReadings.Max(c => c.TempAvg).Round(0).ToInt(),
                    AverageTemp = weatherReadings.Average(c => c.TempAvg).Round(0).ToInt()
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