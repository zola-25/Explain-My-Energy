using Energy.Shared;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

public record DemoMeter
{
    public Guid GlobalId { get; init; }
    public MeterType MeterType { get; init; }
    public string Mpxn { get; init; }
}
