using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Household
{
    [FeatureState(Name = nameof(HouseholdState))]
    [PersistState, PriorityLoad]
    public record HouseholdState
    {
        public bool Saved { get; init; }
        public bool Invalid { get; init; }
        public DateTime? MoveInDate { get; init; }

        public string IhdMacId { get; init; }
        public string OutCodeCharacters { get; init; }

        public MeterType PrimaryHeatSource { get; init; }

        public HouseholdState()
        {
            OutCodeCharacters = null;
            IhdMacId = null;
            Invalid = false;
            MoveInDate = null;
            PrimaryHeatSource = MeterType.Gas;
            Saved = false;
        }
    }



}
