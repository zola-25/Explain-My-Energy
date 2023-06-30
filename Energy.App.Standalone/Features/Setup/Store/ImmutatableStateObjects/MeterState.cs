using Energy.Shared;

namespace Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;

public record MeterState
{
    public Guid GlobalId { get; init; }
    public MeterType MeterType { get; init; }

    public string Mpxn { get; init; }

    public bool InitialSetupValid { get; init; }

    public bool Authorized { get; init; }


    public bool SetupValid { get; init; }

}