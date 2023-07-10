namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityUpdateReadingsAndReloadCostsAction
    {
        public DateTime LastBasicReading { get; }

        public ElectricityUpdateReadingsAndReloadCostsAction(DateTime lastReading)
        {
            LastBasicReading = lastReading;
        }
    }

    public class ElectricityUpdateCostsAction
    {
        public DateTime LastCostReading { get; }

        public ElectricityUpdateCostsAction(DateTime lastCostReading)
        {
            LastCostReading = lastCostReading;
        }
    }
}
