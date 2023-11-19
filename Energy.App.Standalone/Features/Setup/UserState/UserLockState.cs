using Fluxor;
using Fluxor.Persist.Storage;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Setup.UserState;

[FeatureState(Name = nameof(UserLockState))]
[PersistState, PriorityLoad(0)]
public record UserLockState
{
    [property: JsonIgnore]
    public bool Locking { get; init; }

    [property: JsonIgnore]
    public bool Unlocking { get; init; }
    public bool SetupDataLocked { get; init; }

    [property: JsonIgnore]
    public bool LockingOrLocked => Unlocking || Locking || SetupDataLocked;

    public UserLockState()
    {
        Unlocking = false;
        Locking = false;
        SetupDataLocked = false;
    }
}

public enum SetupDataType
{
    IHD,
    GasMeterId,
    ElectricityMeterId,
    OutCode,
}
