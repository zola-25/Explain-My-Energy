﻿using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions
{
    public partial class AuthorizeMeterAction
    {
        public MeterType MeterType { get; init; }

        public AuthorizeMeterAction(MeterType meterType)
        {
            MeterType = meterType;
        }

        [ReducerMethod]
        public static MeterSetupState OnMeterAuthorizingReducer(MeterSetupState meterSetupState, AuthorizeMeterAction action)
        {

            switch (action.MeterType)
            {
                case MeterType.Electricity:
                    return meterSetupState with
                    {
                        ElectricityMeter = meterSetupState[action.MeterType] with
                        {
                            Authorizing = true,
                            Authorized = false,
                            SetupValid = false
                        }
                    };
                case MeterType.Gas:
                    return meterSetupState with
                    {
                        GasMeter = meterSetupState[action.MeterType] with
                        {
                            Authorizing = true,
                            Authorized = false,
                            SetupValid = false
                        }
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.MeterType), action.MeterType, null);

            }

        }

        private class AuthorizeMeterEffect : Effect<AuthorizeMeterAction>
        {
            private readonly IMeterAuthorizationCheck _meterAuthorizationCheck;
            private readonly IDispatcher _dispatcher;
            private readonly IState<HouseholdState> _householdState;

            public AuthorizeMeterEffect(IMeterAuthorizationCheck meterAuthorizationCheck,
                                        IDispatcher dispatcher,
                                        IState<HouseholdState> householdState)
            {
                _meterAuthorizationCheck = meterAuthorizationCheck;
                _dispatcher = dispatcher;
                _householdState = householdState;
            }

            public override async Task HandleAsync(AuthorizeMeterAction action, IDispatcher dispatcher)
            {
                var ihdMacId = _householdState.Value.IhdMacId;
                var result = await _meterAuthorizationCheck.TestAccess(action.MeterType, ihdMacId);

                _dispatcher.Dispatch(new AuthorizeMeterResponseAction(action.MeterType, result.FailureReason, result.Success));
            }
        }
    }


}
