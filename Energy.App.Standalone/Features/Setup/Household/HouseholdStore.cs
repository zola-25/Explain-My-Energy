using Energy.App.Standalone.FluxorPersist;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Household
{
    [FeatureState(Name = nameof(HouseholdState), CreateInitialStateMethodName = nameof(GetInitialState))]
    [PersistState, PriorityLoad]
    public record HouseholdState
    {
        public bool Saved { get; init; }
        public bool Invalid { get; init; }
        public DateTime? MoveInDate { get; init; }

        public string IhdMacId { get; init; }
        public string OutCodeCharacters { get; init; }

        public MeterType PrimaryHeatSource { get; init; }

        public static HouseholdState GetInitialState() {
            return CreateUtilities.Create();
        }
    }

    public static class CreateUtilities {
        public static HouseholdState Create() {
            
            if(ConfigurationHelper.IsDemoSetup) 
            {
                return SetDefaultLocalState.HouseholdState;
            }
            
            return new HouseholdState() {
                Saved = false,
                Invalid = false,
                MoveInDate = null,
                IhdMacId = null,
                OutCodeCharacters = null,
                PrimaryHeatSource = MeterType.Gas
            };
        }

    }

}
