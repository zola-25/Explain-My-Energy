namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IMeterApi
{
    Task<Meter> AddMeterAsync(Meter meter);
    Task<Meter> GetMeterAsync(Guid globalId);
    Task<Dictionary<MeterType, Meter>> GetAllMetersAsync(CancellationToken ctx = default);

    Task<TestAccessResponse> CheckAuthorization(MeterType meterMeterType);
    Task<Meter> Authorize(Guid globalId);
    Task<Meter> StartImport(Guid globalId);
    Task RemoveMeterAsync(Guid globalId);
    Task<Meter> UpdateTariffDetails(Guid globalId, List<TariffDetail> tariffDetails);

    Task<Meter> AddOrUpdateTariff(Guid globalId, TariffDetail tariffDetail);
}