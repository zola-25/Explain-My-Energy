using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

class ForecastCoefficientsCreator : IForecastCoefficientsCreator
{
    public LinearCoefficients GetForecastCoefficients(ICollection<DailyConsumptionPoint> dailyConsumptionPoints)
    {
        IReadOnlyCollection<DailyConsumptionPoint> winterData = GetOctAprilDailyData(dailyConsumptionPoints);

        object[] x = winterData.Select(c => c.TemperatureCelsius).ToArray();
        object[] yConsumption = winterData.Select(c => c.ConsumptionKWh).ToArray();

        var consumptionFit = Fit.Line(x, yConsumption);
        var consumptionCoefficients = new LinearCoefficients()
        {
            C = consumptionFit.A,
            Gradient = consumptionFit.B
        };
        return consumptionCoefficients;
    }

    private IReadOnlyCollection<DailyConsumptionPoint> GetOctAprilDailyData(ICollection<DailyConsumptionPoint> dailyConsumptionPoints)
    {
        List<int> winterMonths = new List<int>() { 1, 2, 3, 4, 10, 11, 12 };
        System.Collections.ObjectModel.ReadOnlyCollection<DailyConsumptionPoint> winterData = dailyConsumptionPoints
            .Where(c => winterMonths.Contains(c.Date.Month)).ToList().AsReadOnly();

        return winterData;
    }
}