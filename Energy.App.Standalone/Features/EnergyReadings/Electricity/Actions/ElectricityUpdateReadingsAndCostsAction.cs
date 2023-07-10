namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class ElectricityUpdateReadingsAndCostsAction
    {
        public DateTime LastReading { get; }

        public ElectricityUpdateReadingsAndCostsAction(DateTime lastReading)
        {
            LastReading = lastReading;
        }
    }


}
