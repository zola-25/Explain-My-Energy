using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
using Energy.Shared;
using System;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Data.EnergyReadings;

public class DemoEnergyReadingRetriever : IEnergyReadingRetriever
{
    readonly ISetDefaultLocalState _demoLocalState;

    public DemoEnergyReadingRetriever(ISetDefaultLocalState setDefaultLocalState)
    {
        _demoLocalState = setDefaultLocalState;
    }

    public async Task<List<BasicReading>> GetMeterReadings(DateTime startDate, DateTime endDate, string macId, MeterType meterType)
    {
        ImmutableList<BasicReading> existingDemoReadings = null;
        switch (meterType)
        {
            case MeterType.Gas:
                if (_demoLocalState.DemoGasReadings == null)
                {
                    await _demoLocalState.LoadDefaultsIfDemo();
                }
                existingDemoReadings = _demoLocalState.DemoGasReadings.BasicReadings;
                break;
            case MeterType.Electricity:
                if (_demoLocalState.DemoElectricityReadings == null)
                {
                    await _demoLocalState.LoadDefaultsIfDemo();
                }
                existingDemoReadings = _demoLocalState.DemoElectricityReadings.BasicReadings;
                break;
            default:
                throw new NotImplementedException();
        }

        var requestedDemoReadings = existingDemoReadings.Where(x => x.UtcTime >= startDate && x.UtcTime <= endDate).ToList();

        var demoReadingsLookup = GetDemoReadingsLookup(existingDemoReadings);

        if (requestedDemoReadings.Last().UtcTime < endDate)
        {
            var random = new Random();

            // Get last year's data with slight variance for any future missing data
            var readingDatesMissing = requestedDemoReadings.Last().UtcTime.eGenerateAllDatesBetween(endDate, true).ToList();
            var demoMissingBasicReadings = readingDatesMissing.SelectMany(x => {

                return Enumerable.Range(0, 48).Select(i => {

                    var missingReadingDateTime = x.AddTicks(TimeSpan.TicksPerMinute * 30 * i);

                    var missingReadingTimeSpan = missingReadingDateTime - new DateTime(missingReadingDateTime.Year,1,1);

                    var lastYearsReading = demoReadingsLookup[missingReadingTimeSpan];


                    decimal demoReadingKWh = 0;

                    double randomFactor = 1 + (random.NextDouble() * 0.2 * (random.Next(2) == 1 ? 1 : -1));
                    demoReadingKWh = lastYearsReading.KWh * (decimal)randomFactor; // just repeat historical zero KWh readings

                    return new BasicReading() {
                        UtcTime = missingReadingDateTime,
                        KWh = demoReadingKWh,
                        Forecast = false
                    };
                });


            }).ToList();

            // ensure we don't include double entries for the same date/time

            demoMissingBasicReadings.RemoveAll(c => c.UtcTime <= requestedDemoReadings.Last().UtcTime);
            requestedDemoReadings.AddRange(demoMissingBasicReadings);
        }
        return requestedDemoReadings;
    }

    private static Dictionary<TimeSpan, BasicReading> GetDemoReadingsLookup(IEnumerable<BasicReading> demoReadings)
    {
        var beginningOfDemoDataYear = demoReadings.First().UtcTime;

        var oneYearOfDemoReadings = demoReadings
            .Where(c => c.UtcTime < beginningOfDemoDataYear.AddYears(1).AddTicks(-(TimeSpan.TicksPerMinute * 30)))
            .ToList();

        return oneYearOfDemoReadings.ToDictionary(c => c.UtcTime - beginningOfDemoDataYear);
    }
}
