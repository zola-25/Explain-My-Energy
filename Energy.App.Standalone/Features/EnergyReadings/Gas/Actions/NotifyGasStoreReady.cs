namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class NotifyGasStoreReady
    {
        public int NumberOfUpdatedReadings { get; }
        public int NumberOfUpdatedCosts { get; }

        public NotifyGasStoreReady(int numberOfUpdatedReadings, int numberOfUpdatedCosts)
        {
            NumberOfUpdatedReadings = numberOfUpdatedReadings;
            NumberOfUpdatedCosts = numberOfUpdatedCosts;
        }
    }

}