namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class ElectricityUpdateLastReadingsCheckAction
{
    public DateTime LastCheckedForNewReadings { get; }

    public ElectricityUpdateLastReadingsCheckAction(DateTime lastCheckedForNewReadings)
    {
        LastCheckedForNewReadings = lastCheckedForNewReadings;
    }
}