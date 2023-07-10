namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class NotifyElectricityStoreReady { 
        public int NumberOfUpdatedReadings { get; }
        public int NumberOfUpdatedCosts { get; }
        
        public NotifyElectricityStoreReady(int numberOfUpdatesReadings, int numberOfUpdatesCosts)
        {
            NumberOfUpdatedReadings = numberOfUpdatesReadings;
            NumberOfUpdatedCosts = numberOfUpdatesCosts;
        }
    }


}
