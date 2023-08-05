using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions;

public class AuthorizeMeterResponseAction
{
    public MeterType MeterType { get; init; }
    public bool AuthorizeSuccess { get; init; }
    public string FailureMessage { get; init; }

    public AuthorizeMeterResponseAction(MeterType meterType, string failureMessage, bool authorizeSuccess)
    {
        MeterType = meterType;
        FailureMessage = failureMessage;
        AuthorizeSuccess = authorizeSuccess;
    }

    [ReducerMethod]
    public static MeterSetupState OnMeterAuthorizeResponseReducer(MeterSetupState meterSetupState, AuthorizeMeterResponseAction action)
    {
        var meterState = meterSetupState[action.MeterType];
        return action.MeterType switch
        {
            MeterType.Electricity => meterSetupState with
            {
                ElectricityMeter = meterState with
                {
                    Authorized = action.AuthorizeSuccess,
                    AuthorizeFailed = !action.AuthorizeSuccess,
                    Authorizing = false,
                    SetupValid = action.AuthorizeSuccess && meterState.InitialSetupValid,
                    AuthorizeFailedMessage = action.FailureMessage
                }
            },
            MeterType.Gas => meterSetupState with
            {
                GasMeter = meterState with
                {
                    Authorized = action.AuthorizeSuccess,
                    AuthorizeFailed = !action.AuthorizeSuccess,
                    Authorizing = false,
                    SetupValid = action.AuthorizeSuccess && meterState.InitialSetupValid,
                    AuthorizeFailedMessage = action.FailureMessage
                }
            },
            _ => throw new ArgumentException(nameof(action.MeterType)),
        };
    }

    private class AuthorizeMeterResponseEffect : Effect<AuthorizeMeterResponseAction>
    {
        private readonly IState<MeterSetupState> _meterSetupState;

        public AuthorizeMeterResponseEffect(IState<MeterSetupState> meterSetupState)
        {
            _meterSetupState = meterSetupState;
        }

        public override Task HandleAsync(AuthorizeMeterResponseAction action, IDispatcher dispatcher)
        {
            if (action.AuthorizeSuccess && _meterSetupState.Value[action.MeterType].InitialSetupValid)
            {
                switch (action.MeterType)
                {
                    case MeterType.Gas:
                        dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(true));
                        break;
                    case MeterType.Electricity:
                        dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(true));
                        break;
                    default:
                        throw new ArgumentException(nameof(action.MeterType));
                }
            }
            return Task.CompletedTask;

        }
    }
}