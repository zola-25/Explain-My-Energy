﻿using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

public record DemoElectricityReadings
{
    public ImmutableList<BasicReading> BasicReadings { get; init; }
}
