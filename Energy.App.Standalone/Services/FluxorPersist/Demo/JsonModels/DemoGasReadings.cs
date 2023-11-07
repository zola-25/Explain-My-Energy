using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

public record DemoGasReadings
{
    public ImmutableList<BasicReading> BasicReadings { get; init; }
    public ImmutableList<CostedReading> CostedReadings { get; init; }
}
