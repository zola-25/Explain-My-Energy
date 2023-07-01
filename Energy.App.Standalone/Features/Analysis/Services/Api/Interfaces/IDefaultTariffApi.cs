namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IDefaultTariffApi
{
    Task<DefaultTariffDetail> GetCurrentExampleTariff(MeterType meterType, ExampleTariffType exampleTariffType, CancellationToken ctx = default);
    Task<List<DefaultTariffDetail>> GetDefaultTariffs(MeterType meterType, ExampleTariffType exampleTariffType, CancellationToken ctx = default);
}