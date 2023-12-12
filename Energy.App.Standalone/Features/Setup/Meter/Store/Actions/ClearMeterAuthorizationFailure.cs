using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class ClearMeterAuthorizationFailure
{
    public MeterType MeterType { get; init; }

    public ClearMeterAuthorizationFailure(MeterType meterType)
    {
        MeterType = meterType;
    }

    [ReducerMethod]
    public static MeterSetupState OnClearMeterAuthorizationFailureReducer(MeterSetupState meterSetupState, ClearMeterAuthorizationFailure action)
    {

        return action.MeterType switch
        {
            MeterType.Electricity => meterSetupState with
            {
                ElectricityMeter = meterSetupState[action.MeterType] with
                {
                    AuthorizeFailed = false,
                    AuthorizeFailedMessage = null
                }
            },
            MeterType.Gas => meterSetupState with
            {
                GasMeter = meterSetupState[action.MeterType] with
                {
                    AuthorizeFailed = false,
                    AuthorizeFailedMessage = null
                }
            },
            _ => throw new ArgumentException(nameof(action.MeterType)),
        };
    }
}
