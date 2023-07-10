namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class GasUpdateReadingsAndCostsAction
    {
        public DateTime LastReading { get; }

        public GasUpdateReadingsAndCostsAction(DateTime lastReading)
        {
            LastReading = lastReading;
        }
    }

}