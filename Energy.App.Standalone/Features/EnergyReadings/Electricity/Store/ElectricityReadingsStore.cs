using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using static Energy.App.Standalone.Features.EnergyReadings.Gas.GasReadingsReducers;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Store
{
    [PersistState]
    public record ElectricityReadingsState
    {
        [property: JsonIgnore]
        public bool ReloadingReadings { get; init; }

        [property: JsonIgnore]
        public bool UpdatingReadings { get; init; }

        public DateTime LastUpdated { get; init; }

        [property: JsonIgnore]
        public ImmutableList<CostedReading> CostedReadings { get; init; }
        
        public ImmutableList<BasicReading> BasicReadings { get; init; }


        [property: JsonIgnore]
        public bool CalculationError { get; init; }
        public bool UpdatingCosts { get; internal set; }
        public bool ReloadingCosts { get; internal set; }
    }

    public class ElectricityReadingsFeature : Feature<ElectricityReadingsState>
    {
        public override string GetName()
        {
            return nameof(ElectricityReadingsFeature);
        }

        protected override ElectricityReadingsState GetInitialState()
        {
            return new ElectricityReadingsState
            {
                ReloadingReadings = false,
                UpdatingReadings = false,
                CalculationError = false,
                CostedReadings = ImmutableList<CostedReading>.Empty,
                BasicReadings = ImmutableList<BasicReading>.Empty,
                LastUpdated = DateTime.MinValue
            };
        }
    }


}
