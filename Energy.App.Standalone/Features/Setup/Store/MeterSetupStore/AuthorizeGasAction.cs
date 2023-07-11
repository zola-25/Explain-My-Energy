using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore
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
