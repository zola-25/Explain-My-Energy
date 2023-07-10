namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasUpdateReadingsAndReloadCostsAction
    {
        public DateTime LastReading { get; }

        public GasUpdateReadingsAndReloadCostsAction(DateTime lastReading)
        {
            LastReading = lastReading;
        }
    }

    public class GasUpdateCostsAction
    {
        public DateTime LastCostReading { get; }

        public GasUpdateCostsAction(DateTime lastCostReading)
        {
            LastCostReading = lastCostReading;
        }
    }

}