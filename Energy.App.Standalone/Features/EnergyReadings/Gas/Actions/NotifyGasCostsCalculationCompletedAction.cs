namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class NotifyGasCostsCalculationCompletedAction
    {
        public bool CalculationError { get; }

        public NotifyGasCostsCalculationCompletedAction(bool calculationError = false)
        {
            CalculationError = calculationError;
        }
    }

}