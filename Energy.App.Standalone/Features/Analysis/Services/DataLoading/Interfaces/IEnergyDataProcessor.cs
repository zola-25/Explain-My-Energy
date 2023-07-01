namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;

public interface IEnergyDataProcessor
{
    Task UpdateData(Meter meter, CancellationToken ctx = default);
    Task ReloadData(Meter meter, CancellationToken ctx = default);
    Task RemoveData(Meter meter);
}