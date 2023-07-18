namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;

public record Coefficients
{
    public decimal Gradient { get; init; }
    public decimal C { get; init; }
    public decimal PredictConsumptionY(decimal xTemperature)
    {
        return Gradient * xTemperature + C;
    }
}




