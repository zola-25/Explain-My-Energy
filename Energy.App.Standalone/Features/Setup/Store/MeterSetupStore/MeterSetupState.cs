﻿using Energy.App.Standalone.Extensions;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Text.Json.Serialization;
using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore;

[FeatureState]
[PersistState, PriorityLoad]
public record MeterSetupState
{
    public MeterState GasMeter { get; init; }
    public MeterState ElectricityMeter { get; init; }

    [property: JsonIgnore]
    public IEnumerable<MeterState> MeterStates => new MeterState[] { GasMeter, ElectricityMeter }.eToIEnumerable();

    public MeterState this[MeterType meterType]
    {
        get
        {
            switch (meterType)
            {
                case MeterType.Gas:
                    return GasMeter;
                case MeterType.Electricity:
                    return ElectricityMeter;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
            }
        }
    }

    public MeterSetupState()
    {

        GasMeter = Utilities.GetMeterInitialState(MeterType.Gas);


        ElectricityMeter = Utilities.GetMeterInitialState(MeterType.Electricity);
    }

}