using System.Text.Json.Serialization;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Meter.Store;

[FeatureState(Name = nameof(MeterSetupState))]
[PersistState, PriorityLoad]
public record MeterSetupState
{
    public MeterState GasMeter { get; init; }
    public MeterState ElectricityMeter { get; init; }

    [property: JsonIgnore]
    public IEnumerable<MeterState> MeterStates => new MeterState[] { GasMeter, ElectricityMeter }.eToIEnumerable();

    public MeterState this[MeterType meterType]
    {
        get {
            return meterType switch
            {
                MeterType.Gas => GasMeter,
                MeterType.Electricity => ElectricityMeter,
                _ => throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null),
            };
        }
    }

    public MeterSetupState()
    {

        GasMeter = Utilities.GetMeterInitialState(MeterType.Gas);


        ElectricityMeter = Utilities.GetMeterInitialState(MeterType.Electricity);
    }
}