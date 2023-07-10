namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;


public class NotifyElectricityCostsCalculationCompletedAction
    {
        public bool Failed { get; }

        public NotifyElectricityCostsCalculationCompletedAction(bool failed = false)
        {
            Failed = failed;
        }
    }
