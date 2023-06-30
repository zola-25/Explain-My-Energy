using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings.Interfaces;

public interface IEnergyReadingImporter
{
    Task<List<BasicReading>> ImportFromMoveIn(MeterType meterType, CancellationToken ctx = default);
    Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default);
}