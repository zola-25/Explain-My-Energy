using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions
{
    public class AuthorizeGasAction
    {

        [ReducerMethod]
        public static MeterSetupState OnGasMeterAuthorizationReducer(MeterSetupState meterSetupState, AuthorizeGasAction authorizationAction)
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
    }


}
