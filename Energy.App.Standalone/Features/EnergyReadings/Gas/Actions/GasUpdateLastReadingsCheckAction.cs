namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class GasUpdateLastReadingsCheckAction
{
    public DateTime LastCheckedForNewReadings { get; }

    public GasUpdateLastReadingsCheckAction(DateTime lastCheckedForNewReadings)
    {
        LastCheckedForNewReadings = lastCheckedForNewReadings;
    }
}