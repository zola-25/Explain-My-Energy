using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    [FeatureState(Name = nameof(GasReadingsState))]
    [PersistState]
    public record GasReadingsState
    {
        [property: JsonIgnore] 
        public bool Loading { get; init; }

        public ImmutableList<BasicReading> BasicReadings { get; init; }

        [property: JsonIgnore]
        public ImmutableList<CostedReading> CostedReadings { get; init; }
        public DateTime LastUpdated { get; init; }

        public GasReadingsState()
        {
            Loading = false;
            CostedReadings = ImmutableList<CostedReading>.Empty;
            BasicReadings = ImmutableList<BasicReading>.Empty;
            LastUpdated = DateTime.MinValue;
        }
    }
}