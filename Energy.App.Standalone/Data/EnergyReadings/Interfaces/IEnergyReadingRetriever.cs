using Energy.Shared;
using System.Runtime.CompilerServices;

namespace Energy.App.Standalone.Data.EnergyReadings.Interfaces;

public interface IEnergyReadingRetriever
{
    Task<List<BasicReading>> GetMeterReadings(DateTime startDate, DateTime endDate, string macId, MeterType meterType);
}