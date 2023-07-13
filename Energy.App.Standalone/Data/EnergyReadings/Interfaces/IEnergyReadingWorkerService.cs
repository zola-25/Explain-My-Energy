using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings.Interfaces;

public interface IEnergyReadingWorkerService
{
    Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default);
    Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default);
}