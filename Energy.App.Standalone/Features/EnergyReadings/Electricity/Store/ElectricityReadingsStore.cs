using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
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

        public ImmutableList<CstR> CostedReadings { get; init; }

        [property: JsonIgnore]
        public bool CalculationError { get; init; }
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
                CostedReadings = ImmutableList<CstR>.Empty
            };
        }
    }


}
