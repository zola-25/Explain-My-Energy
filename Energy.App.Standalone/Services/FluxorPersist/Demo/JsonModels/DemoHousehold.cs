using Energy.Shared;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

public record DemoHousehold
{
    public DateTime? MoveInDate { get; init; }

    public string IhdMacId { get; init; }
    public string OutCodeCharacters { get; init; }

    public MeterType PrimaryHeatSource { get; init; }
}
