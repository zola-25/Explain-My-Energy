using Energy.App.Blazor.Client.StateContainers;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class TempForecastAnalyzer : ITempForecastAnalyzer
{
    private readonly Co2ConversionFactors _co2Conversion;
    private readonly ITermDateRanges _periodDateRanges;
    private readonly WeatherDataState _weatherDataState;
    private readonly ForecastState _forecastState;

    public TempForecastAnalyzer(ITermDateRanges periodDateRanges,
                                WeatherDataState weatherDataState,
                                ForecastState forecastState)
    {
        _periodDateRanges = periodDateRanges;
        _weatherDataState = weatherDataState;
        _co2Conversion = new Co2ConversionFactors();
        _forecastState = forecastState;
    }



    public ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType,
        CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetNextPeriodDates(duration);
        var forecastCosts = _forecastState.GetTempDiffForecast(meterType);

        var weatherReadings = _weatherDataState.WeatherReadings.Where(c => c.ReadDate >= start && c.ReadDate < end).ToList();
        var costReadings = forecastCosts.Where(c => c.LocalTime >= start && c.LocalTime < end).ToList();

        ForecastAnalysis results = ForecastAnalysis(meterType, costReadings, weatherReadings);

        return results;
    }

    public ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
        CalendarTerm duration)
    {
        (DateTime start, DateTime end) = _periodDateRanges.GetCurrentPeriodDates(duration);
        var forecastCosts = _forecastState.GetTempDiffForecast(meterType);

        var weatherReadings = _weatherDataState.WeatherReadings.Where(c => c.ReadDate >= start && c.ReadDate < end).ToList();
        var costReadings = forecastCosts.Where(c => c.LocalTime >= start && c.LocalTime < end).ToList();

        ForecastAnalysis results = ForecastAnalysis(meterType, costReadings, weatherReadings);

        return results;
    }



    private ForecastAnalysis ForecastAnalysis(
        MeterType meterType,
        ICollection<CostedReading> costedReadings,
        ICollection<DailyWeatherReading> dailyWeatherReadings)
    {
        double forecastConsumption = costedReadings.Sum(c => c.KWh);

        ForecastAnalysis results = new ForecastAnalysis()
        {
            NumberOfDays = dailyWeatherReadings.Count,
            Start = dailyWeatherReadings.First().ReadDate,
            End = dailyWeatherReadings.Last().ReadDate,
            ForecastConsumption = forecastConsumption.Round(0),
            ForecastCostPence = costedReadings.Sum(c => c.CostPence).Round(2),
            ForecastCo2 = (forecastConsumption * _co2Conversion.GetCo2ConversionFactor(meterType)).Round(1),
            TemperatureRange = new TemperatureRange()
            {
                LowDailyTemp = Convert.ToInt32(dailyWeatherReadings.Min(c => c.TemperatureMeanHourly)),
                HighDailyTemp = Convert.ToInt32(dailyWeatherReadings.Max(c => c.TemperatureMeanHourly)),
                AverageTemp = Convert.ToInt32(dailyWeatherReadings.Average(c => c.TemperatureMeanHourly))
            }
        };
        return results;
    }

}