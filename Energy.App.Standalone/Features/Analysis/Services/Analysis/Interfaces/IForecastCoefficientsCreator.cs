namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IForecastCoefficientsCreator
{
    LinearCoefficients GetForecastCoefficients(ICollection<DailyConsumptionPoint> dailyConsumptionPoints);
}