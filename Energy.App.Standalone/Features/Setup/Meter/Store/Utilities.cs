using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Meter.Store;

public static class Utilities
{
    public static MeterState GetMeterInitialState(MeterType meterType)
    {

        return new MeterState
        {
            GlobalId = Guid.Empty,
            Authorized = false,
            Authorizing = false,
            AuthorizeFailed = false,
            MeterType = meterType,
            Mpxn = null,
            SetupValid = false,
            TariffDetails = ImmutableList<TariffDetailState>.Empty,
            InitialSetupValid = false,
            ProblemMessage = null,
        };
    }

}