//using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
//using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
//using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

//namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

//public class SimpleForecastAnalyzer : ISimpleForecastAnalyzer
//{
//    private readonly Co2ConversionFactors _co2Conversion;
//    private readonly ITermDateRanges _periodDateRanges;
//    private readonly HistoricalAverageForecastState _historicalAverageForecastState;


//    public SimpleForecastAnalyzer(ITermDateRanges periodDateRanges, HistoricalAverageForecastState historicalAverageForecastState)
//    {
//        _periodDateRanges = periodDateRanges;
//        _historicalAverageForecastState = historicalAverageForecastState;
//        _co2Conversion = new Co2ConversionFactors();
//    }

//    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm duration)
//    {
//        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(duration);

//        var forecastCosts = _historicalAverageForecastState.GetHistoricalAverageForecast(meterType);

//        var costReadings = forecastCosts.Where(c => c.LocalTime >= start && c.LocalTime < end).ToList();

//        ForecastAnalysis results = ForecastAnalysis(meterType, costReadings);

//        return results;
//    }

//    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
//        CalendarTerm duration)
//    {
//        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);
//        var forecastCosts = _historicalAverageForecastState.GetHistoricalAverageForecast(meterType);

//        var costReadings = forecastCosts.Where(c => c.LocalTime >= start && c.LocalTime < end).ToList();

//        return ForecastAnalysis(meterType, costReadings);
//    }


//    private ForecastAnalysis ForecastAnalysis(MeterType meterType, ICollection<CostedReading> costedReadings)
//    {
//        double forecastConsumption = costedReadings.Sum(c => c.KWh);
//        int numOfDays = costedReadings.GroupBy(c => c.LocalTime.Date).Count();

//        ForecastAnalysis results = new ForecastAnalysis()
//        {
//            NumberOfDays = numOfDays,
//            Start = costedReadings.First().LocalTime.Date,
//            End = costedReadings.First().LocalTime.Date,
//            ForecastConsumption = forecastConsumption.Round(0),
//            ForecastCostPence = costedReadings.Sum(c => c.CostPence).Round(2),
//            ForecastCo2 = (forecastConsumption * _co2Conversion.GetCo2ConversionFactor(meterType)).Round(1),
//            TemperatureRange = new TemperatureRange()
//        };
//        return results;
//    }

//}