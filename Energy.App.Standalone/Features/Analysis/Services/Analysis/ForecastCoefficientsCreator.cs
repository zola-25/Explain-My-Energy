using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.Shared;
using MathNet.Numerics;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

class ForecastCoefficientsCreator : IForecastCoefficientsCreator
{
    public (decimal C, decimal Gradient) GetForecastCoefficients(
        IEnumerable<BasicReading> basicReadings,
        IEnumerable<DailyWeatherReading> dailyWeatherReadings)
    {

        var dailyConsumptionPoints =
                (from er in basicReadings
                 group new { Readings = er } by er.UtcTime.Date
                    into daily
                 join wr in dailyWeatherReadings on daily.Key equals wr.UtcTime
                 select new DailyConsumptionPoint
                 {
                     Date = daily.Key,
                     TemperatureCelsius = (double)wr.TemperatureAverage,
                     ConsumptionKWh = (double)(daily.Sum(c => c.Readings.KWh))
                 }
                ).ToList();


        var winterData = GetOctAprilDailyData(dailyConsumptionPoints);

        double[] x = winterData.Select(c => c.TemperatureCelsius).ToArray();
        double[] yConsumption = winterData.Select(c => c.ConsumptionKWh).ToArray();

        var consumptionFit = Fit.Line(x, yConsumption);

        return (C: (decimal)consumptionFit.A, Gradient: (decimal)consumptionFit.B);
    }

    private List<DailyConsumptionPoint> GetOctAprilDailyData(ICollection<DailyConsumptionPoint> dailyConsumptionPoints)
    {
        List<int> winterMonths = new List<int>() { 1, 2, 3, 4, 10, 11, 12 };
        var winterData = dailyConsumptionPoints
            .Where(c => winterMonths.Contains(c.Date.Month)).ToList();

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