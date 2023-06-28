using Energy.App.Standalone.Models;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store
{
    public record MeterSetupState
    {
        public MeterState GasMeter { get; init; }
        public MeterState ElectricityMeter { get; init; }

    }

    public class MeterSetupFeature : Feature<MeterSetupState>
    {
        public override string GetName()
        {
            return nameof(MeterSetupFeature);
        }

        protected override MeterSetupState GetInitialState()
        {
            return new MeterSetupState
            {
                GasMeter = new MeterState
                {
                    GlobalId = Guid.Empty,
                    Authorized = false,
                    MeterType = MeterType.Gas,
                    Mpxn = null,
                    SetupValid = false
                },
                ElectricityMeter = new MeterState
                {
                    GlobalId = Guid.Empty,
                    Authorized = false,
                    MeterType = MeterType.Electricity,
                    Mpxn = null,
                    SetupValid = false
                },
            };
        }
    }

    

    public class MeterAddSuccessAction
    {
        public Meter Meter { get; }

        public MeterType MeterType => Meter.MeterType;


        public MeterAddSuccessAction(Meter meter)
        {
            Meter = meter;
        }
    }

    public class MeterUpdateSuccessAction
    {
        public Meter Meter { get; }

        public MeterType MeterType => Meter.MeterType;


        public MeterUpdateSuccessAction(Meter meter)
        {
            Meter = meter;
        }
    }

    public class MeterAddFailureAction
    {
        public MeterType MeterType { get; }

        public MeterAddFailureAction(MeterType meterType)
        {
            MeterType = meterType;
        }

    }


    public class NotifyMeterReadyAction
    {
        public Meter Meter { get; }

        public MeterType MeterType => Meter.MeterType;


        public NotifyMeterReadyAction(Meter meter)
        {
            Meter = meter;
        }
    }


    public static class MeterReducers
    {
        [ReducerMethod]
        public static MeterSetupState OnMeterAddSuccessReducer(MeterSetupState state, MeterAddSuccessAction action)
        {
            if (action.MeterType == MeterType.Electricity)
            {
                return state with
                {
                    ElectricityMeter = action.Meter
                };
            }
            else
            {
                return state with
                {
                    GasMeter = action.Meter
                };
            }

        }

        [ReducerMethod(typeof(HouseholdSubmitFailureAction))]

        public static MeterSetupState OnSubmitFailureReducer(MeterSetupState state)
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
