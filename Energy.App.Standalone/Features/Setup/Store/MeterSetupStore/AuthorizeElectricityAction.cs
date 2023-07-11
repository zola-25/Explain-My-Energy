﻿using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

public class AuthorizeElectricityAction
{

    [ReducerMethod(typeof(AuthorizeElectricityAction))]
    public static MeterSetupState OnElectricityMeterAuthorizationReducer(MeterSetupState meterSetupState)
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
}