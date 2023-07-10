using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
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
                    SetupValid = true
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
                    SetupValid = true
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
        public Task NotifyGasInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasInitiateSetDefaultTariffsAction());

            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(GasMeterInitialUpdateAction))]
        public Task NotifyGasInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyGasMeterInitialSetupValidAction());
            return Task.CompletedTask;

        }

        [EffectMethod(typeof(GasMeterDeleteAction))]
        public Task DispatchGasDeleteAllAssociatedDataActions(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasInitiateDeleteReadingsAction());
            dispatcher.Dispatch(new DeleteAllGasTariffsAction());

            return Task.CompletedTask;

        }

        [EffectMethod(typeof(ElectricityMeterInitialAddAction))]
        public Task NotifyElectricityInitialAddSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityInitiateSetDefaultTariffsAction());

            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
            return Task.CompletedTask;

        }

        [EffectMethod(typeof(ElectricityMeterInitialUpdateAction))]
        public Task NotifyElectricityInitialUpdateSuccess(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new NotifyElectricityMeterInitialSetupValidAction());
            return Task.CompletedTask;

        }

        [EffectMethod(typeof(ElectricityMeterDeleteAction))]
        public Task DispatchElectricityDeleteAllAssociatedData(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityInitiateDeleteReadingsAction());
            dispatcher.Dispatch(new DeleteAllElectricityTariffsAction());

            return Task.CompletedTask;

        }
    }


}
