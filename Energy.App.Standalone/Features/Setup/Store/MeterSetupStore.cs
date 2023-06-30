using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Setup.Store
{
    [PersistState, PriorityLoad]

    public record MeterSetupState
    {
        public MeterState GasMeter { get; init; }
        public MeterState ElectricityMeter { get; init; }

        [property: JsonIgnore]
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

    public class GasMeterDeleteAction
    {
    }

    public class ElectricityMeterDeleteAction
    {
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
            MeterState meterState = meterSetupState.ElectricityMeter;
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
            MeterState meterState = meterSetupState.ElectricityMeter;
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
            MeterState meterState = meterSetupState.GasMeter;
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
            MeterState meterState = meterSetupState.GasMeter;
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
            MeterState meterState = meterSetupState.ElectricityMeter;
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
            MeterState meterState = meterSetupState.GasMeter;
            return meterSetupState with
            {
                GasMeter = meterState with
                {
                    Authorized = true,
                }
            };
        }

        [ReducerMethod]
        public static MeterSetupState OnGasMeterDeleteReducer(MeterSetupState meterSetupState, GasMeterDeleteAction deleteAction)
        {
            MeterState resettedMeterState = Utilities.GetMeterInitialState(MeterType.Gas);
            return meterSetupState with
            {
                GasMeter = resettedMeterState
            };
        }

        [ReducerMethod]
        public static MeterSetupState OnElectricityMeterDeleteReducer(MeterSetupState meterSetupState, ElectricityMeterDeleteAction deleteAction)
        {
            MeterState resettedMeterState = Utilities.GetMeterInitialState(MeterType.Electricity);
            return meterSetupState with
            {
                ElectricityMeter = resettedMeterState
            };
        }

    }
    public class MeterSetupEffects
    {

        [EffectMethod(typeof(GasMeterInitialAddAction))]
        public void NotifyGasInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasInitiateSetDefaultTariffsAction());

            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(GasMeterInitialUpdateAction))]
        public void NotifyGasInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(GasMeterDeleteAction))]
        public void DispatchGasDeleteReadingsActions(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasInitiateDeleteReadingsAction());
        }

        [EffectMethod(typeof(ElectricityMeterInitialAddAction))]
        public void NotifyElectricityInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityInitiateSetDefaultTariffsAction());

            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(ElectricityMeterInitialUpdateAction))]
        public void NotifyElectricityInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
        }

        [EffectMethod(typeof(ElectricityMeterDeleteAction))]
        public void DispatchElectricityDeleteReadingsActions(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityInitiateDeleteReadingsAction());
        }
    }


}
