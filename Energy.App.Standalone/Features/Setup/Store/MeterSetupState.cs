using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Store
{
    [PersistState, PriorityLoad]

    public record MeterSetupState
    {
        public MeterState GasMeter { get; init; }
        public MeterState ElectricityMeter { get; init; }

        public IEnumerable<MeterState> MeterStates => new MeterState[] { GasMeter, ElectricityMeter }.eToIEnumerable();

        public MeterState this[MeterType meterType]
        {
            get
            {
                switch (meterType)
                {
                    case MeterType.Gas:
                        return GasMeter;
                    case MeterType.Electricity:
                        return ElectricityMeter;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
                }
            }
        }

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
                GasMeter = Utilities.GetMeterInitialState(MeterType.Gas),
                ElectricityMeter = Utilities.GetMeterInitialState(MeterType.Electricity),
            };
        }
    }

    public static class Utilities
    {
        public static MeterState GetMeterInitialState(MeterType meterType)
        {

            return new MeterState
            {
                GlobalId = Guid.Empty,
                Authorized = false,
                MeterType = meterType,
                Mpxn = null,
                SetupValid = false
            };
        }

        public static MeterSetupState UpdateForMeter(MeterSetupState meterSetupState, MeterState meterState)
        {
            switch (meterState.MeterType)
            {
                case MeterType.Electricity:
                    return meterSetupState with
                    {
                        ElectricityMeter = meterState
                    };
                case MeterType.Gas:
                    return meterSetupState with
                    {
                        GasMeter = meterState
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterState.MeterType));

            }
        }
    }



    public class GasMeterInitialAddAction
    {
        public Meter Meter { get; }

        public GasMeterInitialAddAction(Meter meter)
        {
            Meter = meter;
        }
    }

    public class GasMeterInitialUpdateAction
    {
        public Meter Meter { get; }

        public GasMeterInitialUpdateAction(Meter meter)
        {
            Meter = meter;
        }
    }

    public class ElectricityMeterInitialAddAction
    {
        public Meter Meter { get; }

        public ElectricityMeterInitialAddAction(Meter meter)
        {
            Meter = meter;
        }
    }

    public class ElectricityMeterInitialUpdateAction
    {
        public Meter Meter { get; }


        public ElectricityMeterInitialUpdateAction(Meter meter)
        {
            Meter = meter;
        }
    }

    

    public class ElectricityMeterSuccessfulAuthorizationAction
    {

    }

    public class GasMeterSuccessfulAuthorizationAction
    {

    }

    public class MeterDeleteAction
    {
        public MeterType MeterType { get; }
        public Guid MeterId { get; }


        public MeterDeleteAction(MeterType meterType, Guid meterId)
        {
            MeterType = meterType;
            MeterId = meterId;
        }
    }



    public class NotifyGasMeterInitialSetupValidAction
    {

    }

    public class NotifyElectricityMeterInitialSetupValidAction
    {

    }


    public static class MeterReducers
    {
        [ReducerMethod]
        public static MeterSetupState OnElectricityMeterInitialAddReducer(MeterSetupState meterSetupState, ElectricityMeterInitialAddAction addSuccessAction)
        {
            var meterState = meterSetupState.ElectricityMeter;
            return meterSetupState with
            {
                ElectricityMeter = meterState with
                {
                    GlobalId = Guid.NewGuid(),
                    InitialSetupValid = true,
                    Authorized = false,
                    MeterType = MeterType.Electricity,
                    Mpxn = addSuccessAction.Meter.Mpxn
                }
            };

        }

        [ReducerMethod]
        public static MeterSetupState OnElectricityMeterInitialUpdateReducer(MeterSetupState meterSetupState, ElectricityMeterInitialUpdateAction updateSuccessAction)
        {
            var meterState = meterSetupState.ElectricityMeter;
            return meterSetupState with
            {
                ElectricityMeter = meterState with
                {
                    InitialSetupValid = true,
                    Authorized = false,
                    MeterType = MeterType.Electricity,
                    Mpxn = updateSuccessAction.Meter.Mpxn
                }
            };

        }

        [ReducerMethod]
        public static MeterSetupState OnGasMeterInitialAddReducer(MeterSetupState meterSetupState, GasMeterInitialAddAction addSuccessAction)
        {
            var meterState = meterSetupState.GasMeter;
            return meterSetupState with
            {
                GasMeter = meterState with
                {
                    GlobalId = Guid.NewGuid(),
                    InitialSetupValid = true,
                    Authorized = false,
                    Mpxn = addSuccessAction.Meter.Mpxn
                }
            };

        }

        [ReducerMethod]
        public static MeterSetupState OnGasMeterInitialUpdateReducer(MeterSetupState meterSetupState, GasMeterInitialUpdateAction updateSuccessAction)
        {
            var meterState = meterSetupState.GasMeter;
            return meterSetupState with
            {
                GasMeter = meterState with
                {
                    InitialSetupValid = true,
                    Authorized = false,
                    Mpxn = updateSuccessAction.Meter.Mpxn
                }
            };
        }

        [ReducerMethod]
        public static MeterSetupState OnElectricityMeterAuthorizationReducer(MeterSetupState meterSetupState, ElectricityMeterSuccessfulAuthorizationAction authorizationAction)
        {
            var meterState = meterSetupState.ElectricityMeter;
            return meterSetupState with
            {
                ElectricityMeter = meterState with
                {
                    Authorized = true,
                }
            };
        }

        [ReducerMethod]
        public static MeterSetupState OnGasMeterAuthorizationReducer(MeterSetupState meterSetupState, GasMeterSuccessfulAuthorizationAction authorizationAction)
        {
            var meterState = meterSetupState.GasMeter;
            return meterSetupState with
            {
                GasMeter = meterState with
                {
                    Authorized = true,
                }
            };
        }

        [ReducerMethod]
        public static MeterSetupState OnMeterDeleteReducer(MeterSetupState meterSetupState, MeterDeleteAction deleteAction)
        {
            MeterState resettedMeterState = Utilities.GetMeterInitialState(deleteAction.MeterType);
            return Utilities.UpdateForMeter(meterSetupState, resettedMeterState);
        }



    }
    public class MeterSetupEffects
    {

        [EffectMethod(typeof(GasMeterInitialAddAction))]
        public async Task NotifyGasInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(GasMeterInitialUpdateAction))]
        public async Task NotifyGasInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(ElectricityMeterInitialAddAction))]
        public async Task NotifyElectricityInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(ElectricityMeterInitialUpdateAction))]
        public async Task NotifyElectricityInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
        }
    }


}
