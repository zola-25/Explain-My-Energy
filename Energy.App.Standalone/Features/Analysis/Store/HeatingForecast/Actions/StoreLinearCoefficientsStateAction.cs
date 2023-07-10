namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class StoreLinearCoefficientsStateAction
    {
        public decimal Gradient { get; }
        public decimal C { get; }

        public StoreLinearCoefficientsStateAction(decimal gradient, decimal c)
        {
            Gradient = gradient;
            C = c;
        }
    }















}
