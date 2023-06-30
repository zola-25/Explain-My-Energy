using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Store
{
    [PersistState, PriorityLoad]
    public record HouseholdState
    {
        public bool Saved { get; init; }
        public bool Invalid { get; init; }
        public bool Validating { get; init; }
        public DateTime? MoveInDate { get; init; }

        public string IhdMacId { get; init; }
        public string OutCodeCharacters { get; init; }

        public MeterType PrimaryHeatSource { get; init; }
    }

    public class HouseholdSetupFeature : Feature<HouseholdState>
    {
        public override string GetName()
        {
            return nameof(HouseholdSetupFeature);
        }

        protected override HouseholdState GetInitialState()
        {
            return new HouseholdState
            {
                OutCodeCharacters = null,
                IhdMacId = null,
                Invalid = false,
                MoveInDate = null,
                PrimaryHeatSource = MeterType.Gas,
                Saved = false,
                Validating = false
            };
        }
    }

    public class HouseholdSubmitSuccessAction
    {
        public DateTime? MoveInDate { get; }

        public string IhdMacId { get; }
        public string OutCodeCharacters { get; }

        public MeterType PrimaryHeatSource { get; }

        public HouseholdSubmitSuccessAction(DateTime? moveInDate, string ihdMacId, string outCodeCharacters, MeterType primaryHeatSource)
        {
            MoveInDate = moveInDate;
            IhdMacId = ihdMacId;
            OutCodeCharacters = outCodeCharacters;
            PrimaryHeatSource = primaryHeatSource;
        }
    }

    public class HouseholdSubmitFailureAction
    {

    }

    public class HouseholdValidatingAction
    {
    }

    public class NotifyHouseholdReadyAction
    {
    }


    public static class HouseholdReducers
    {
        [ReducerMethod(typeof(HouseholdValidatingAction))]

        public static HouseholdState OnValidatingReducer(HouseholdState state)
        {
            return state with
            {
                Validating = true
            };
        }


        [ReducerMethod]

        public static HouseholdState OnSubmitSuccessReducer(HouseholdState state, HouseholdSubmitSuccessAction action)
        {
            return state with
            {
                Validating = false,
                Invalid = false,
                Saved = true,
                OutCodeCharacters = action.OutCodeCharacters,
                IhdMacId = action.IhdMacId,
                MoveInDate = action.MoveInDate,
                PrimaryHeatSource = action.PrimaryHeatSource
            };
        }

        [ReducerMethod(typeof(HouseholdSubmitFailureAction))]

        public static HouseholdState OnSubmitFailureReducer(HouseholdState state)
        {
            return state with
            {
                Validating = false,
                Invalid = true,
            };
        }
    }

    public class HouseholdEffects
    {

        [EffectMethod(typeof(HouseholdSubmitSuccessAction))]
        public async Task NotifyHouseholdReady(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyHouseholdReadyAction());
        }
    }

}
