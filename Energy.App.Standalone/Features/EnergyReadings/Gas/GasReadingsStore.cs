using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    [PersistState]
    public record GasReadingsState
    {
        [property: JsonIgnore] public bool ReloadingReadings { get; init; }


        [property: JsonIgnore] public bool UpdatingReadings { get; init; }

        public DateTime LastUpdated { get; init; }
        public ImmutableList<BasicReading> BasicReadings { get; init; }

        [property: JsonIgnore]
        public ImmutableList<CostedReading> CostedReadings { get; init; }

        [property: JsonIgnore] public bool CalculationError { get; init; }
        public bool ReloadingCosts { get; internal set; }
        public bool UpdatingCosts { get; internal set; }
    }

    public class GasReadingsFeature : Feature<GasReadingsState>
    {
        public override string GetName()
        {
            return nameof(GasReadingsFeature);
        }

        protected override GasReadingsState GetInitialState()
        {
            return new GasReadingsState
            {
                ReloadingReadings = false,
                UpdatingReadings = false,
                CalculationError = false,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                LastUpdated = DateTime.MinValue
            };
        }
    }

}