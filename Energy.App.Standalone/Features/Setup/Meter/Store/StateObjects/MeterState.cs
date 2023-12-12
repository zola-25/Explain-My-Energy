using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;

public record MeterState
{
    public Guid GlobalId { get; init; }
    public MeterType MeterType { get; init; }

    public string Mpxn { get; init; }

    public bool InitialSetupValid { get; init; }

    public bool Authorized { get; init; }
    public bool Authorizing { get; init; }

    public bool AuthorizeFailed { get; init; }
    [property: JsonIgnore]
    public bool SetupValid => InitialSetupValid && Authorized && !AuthorizeFailed && !Authorizing;

    public ImmutableList<TariffDetailState> TariffDetails { get; init; }
    public string AuthorizeFailedMessage { get; init; }
}