using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity;

[FeatureState(Name = nameof(ElectricityReadingsState))]
[PersistState]
public record ElectricityReadingsState
{
    [property: JsonIgnore]
    public bool Loading { get; init; }

    public ImmutableList<BasicReading> BasicReadings { get; init; }


    [property: JsonIgnore]
    public ImmutableList<CostedReading> CostedReadings { get; init; }

    public DateTime LastCheckedForNewReadings { get; init; }

    public ElectricityReadingsState()
    {
        Loading = false;
        CostedReadings = ImmutableList<CostedReading>.Empty;
        BasicReadings = ImmutableList<BasicReading>.Empty;
        LastCheckedForNewReadings = DateTime.MinValue;
    }
}
