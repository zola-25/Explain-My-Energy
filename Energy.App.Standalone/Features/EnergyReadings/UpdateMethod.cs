using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings;


public class UpdateMethod
{

    public UpdateType UpdateType { get; }
    public ImmutableList<BasicReading> BasicReadings { get; }
    public ImmutableList<CostedReading> CostedReadings { get; }
    public int BasicReadingsUpdateCount { get; }


    public UpdateMethod(UpdateType updateType, ImmutableList<BasicReading> basicReadings, ImmutableList<CostedReading> costedReadings, int updateCount)
    {
        UpdateType = updateType;
        BasicReadings = basicReadings;
        CostedReadings = costedReadings;
        BasicReadingsUpdateCount = updateCount;
    }
}
