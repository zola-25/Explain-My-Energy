using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using System.Collections.Immutable;
using MathNet.Numerics;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastAnalyzer : ITempForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly IForecastGenerator _forecastGenerator;
    private readonly ICostCalculator _costCalculator;

    public TempForecastAnalyzer(Co2ConversionFactors co2Conversion,
                                ITermDateRanges periodDateRanges,
                                IForecastGenerator forecastGenerator,
                                ICostCalculator costCalculator)
    {
        _co2Conversion = co2Conversion;
        _periodDateRanges = periodDateRanges;
        _forecastGenerator = forecastGenerator;
        _costCalculator = costCalculator;
    }

    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, 
        CalendarTerm duration,
        decimal degreeDifference,
        LinearCoefficientsState linearCoefficientsState,
        ImmutableList<TariffDetailState> tariffDetailStates,
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(duration);

        ForecastAnalysis results = ForecastAnalysis(meterType,
                                                    degreeDifference,
                                                    start,
                                                    end,
                                                    linearCoefficientsState,
                                                    tariffDetailStates,
                                                    dailyWeatherReadings);

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, 
        CalendarTerm duration,
        decimal degreeDifference,
        LinearCoefficientsState linearCoefficientsState,
        ImmutableList<TariffDetailState> tariffDetailStates,
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);

        ForecastAnalysis results = ForecastAnalysis(meterType,
                                                    degreeDifference,
                                                    start,
                                                    end,
                                                    linearCoefficientsState,
                                                    tariffDetailStates,
                                                    dailyWeatherReadings);

        return results;
    }



    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        decimal degreeDifference,
        DateTime start,
        DateTime end,
        LinearCoefficientsState linearCoeffiecientsState,
        ImmutableList<TariffDetailState> tariffDetailStates,
        ImmutableList<DailyWeatherReading> dailyWeatherReadings)
    {
        var forecastBasicReadings = _forecastGenerator.GetBasicReadingsForecast(start, end, degreeDifference, linearCoeffiecientsState, dailyWeatherReadings);
        var forecastCostedReadings = _costCalculator.GetCostReadings(forecastBasicReadings, tariffDetailStates);

        decimal forecastConsumption = forecastCostedReadings.Sum(c => c.KWh);

        ForecastAnalysis results = new ForecastAnalysis()
        {
            NumberOfDays = dailyWeatherReadings.Count,
            Start = start,
            End = end,
            ForecastConsumption = forecastConsumption.Round(0),
            ForecastCostPounds = (forecastCostedReadings.Sum(c => c.CostPence) / 100m).Round(2),
            ForecastCo2 = (forecastConsumption * _co2Conversion.GetCo2ConversionFactor(meterType)).Round(1),
            TemperatureRange = new TemperatureRange()
            {
                LowDailyTemp = Convert.ToInt32(dailyWeatherReadings.Min(c => c.TemperatureAverage)),
                HighDailyTemp = Convert.ToInt32(dailyWeatherReadings.Max(c => c.TemperatureAverage)),
                AverageTemp = Convert.ToInt32(dailyWeatherReadings.Average(c => c.TemperatureAverage))
            }
        };
        return results;
    }

}