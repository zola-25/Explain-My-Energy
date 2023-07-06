using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings.Interfaces;

public interface IEnergyReadingImporter
{
    Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default);
    Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default);
}