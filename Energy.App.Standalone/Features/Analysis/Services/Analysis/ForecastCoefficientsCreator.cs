using Energy.App.Standalone.Data;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using MathNet.Numerics;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class ForecastCoefficientsCreator : IForecastCoefficientsCreator
{
    public (decimal C, decimal Gradient) GetForecastCoefficients(
        IEnumerable<BasicReading> basicReadings,
        IEnumerable<DailyWeatherRecord> dailyWeatherReadings)
    {

        var dailyConsumptionPoints =
                (from er in basicReadings
                 group new { Readings = er } by er.UtcTime.Date
                    into daily
                 where daily.Count() == 48
                 join wr in dailyWeatherReadings on daily.Key equals wr.Utc
                 select new DailyConsumptionPoint
                 {
                     Date = daily.Key,
                     TemperatureCelsius = (double)wr.TempAvg,
                     ConsumptionKWh = (double)(daily.Sum(c => c.Readings.KWh))
                 }
                ).ToList();


        var winterData = GetOctAprilDailyData(dailyConsumptionPoints);

        double[] x = winterData.Select(c => c.TemperatureCelsius).ToArray();
        double[] yConsumption = winterData.Select(c => c.ConsumptionKWh).ToArray();

        var (A, B) = Fit.Line(x, yConsumption);

        return (C: (decimal)A, Gradient: (decimal)B);
    }

    private List<DailyConsumptionPoint> GetOctAprilDailyData(ICollection<DailyConsumptionPoint> dailyConsumptionPoints)
    {
        var winterData = dailyConsumptionPoints
            .Where(c => AppWideForecastProperties.LowTemperatureMonths.Contains(c.Date.Month)).ToList();

        return winterData;
    }

    private record DailyConsumptionPoint
    {
        public double TemperatureCelsius { get; init; }
        public double ConsumptionKWh { get; init; }
        public DateTime Date { get; init; }
        public double Cost { get; init; }
        public bool IsForecast { get; init; }
    }
}