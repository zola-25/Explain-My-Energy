using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

public record DemoMeterSetup
{
    public DemoMeter ElectricityMeter { get; init; }
    public DemoMeter GasMeter { get; init; }
}
