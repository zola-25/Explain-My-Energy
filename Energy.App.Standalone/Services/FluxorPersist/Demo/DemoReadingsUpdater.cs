using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo;

public class DemoReadingsUpdater : IDemoReadingsUpdater
{
    private readonly ICostCalculator _costCalculator;

    public DemoReadingsUpdater(ICostCalculator costCalculator)
    {
        _costCalculator = costCalculator;
    }

    public UpdatedDemoReadings GetUpdatedReadings(
        DateTime utcNow,
        ImmutableList<BasicReading> jsonDemoReadings,
        ImmutableList<CostedReading> jsonDemoCostedReadings,
        IEnumerable<TariffDetailState> meterTariffs)
    {

        var endDate = utcNow.Date.AddTicks(TimeSpan.TicksPerDay).Date;
        var startDate = endDate.AddYears(-1).AddMonths(-1).Date; // ensure a maximum of 13 months of data

        //var requestedDemoReadings = existingDemoReadings.Where(x => x.UtcTime >= startDate && x.UtcTime <= endDate).ToList();

        var latestJsonDemoReading = jsonDemoReadings.Last().UtcTime;


        var thirteenMonthDemoReadings = jsonDemoReadings.Where(c => c.UtcTime >= startDate).ToList();

        if (latestJsonDemoReading < endDate)
        {
            var random = new Random();

            // Get last year's data with slight variance for any future missing data
            var readingDatesMissing = latestJsonDemoReading.eGenerateAllDatesBetween(endDate, true).ToList();
            var demoMissingBasicReadings = readingDatesMissing.SelectMany(x => {

                return Enumerable.Range(0, 48).Select(i => {

                    var missingReadingDateTime = x.AddTicks(TimeSpan.TicksPerMinute * 30 * i);

                    var lastYearsReading = jsonDemoReadings.Where(c =>
                        c.UtcTime.Month == missingReadingDateTime.Month &&
                        c.UtcTime.Day == missingReadingDateTime.Day &&
                        c.UtcTime.Hour == missingReadingDateTime.Hour &&
                        c.UtcTime.Minute == missingReadingDateTime.Minute).First();


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

            thirteenMonthDemoReadings.AddRange(demoMissingBasicReadings);
        }

        var thirteenMonthDemoCostedReadings = _costCalculator.GetCostReadings(thirteenMonthDemoReadings, meterTariffs);

        return new UpdatedDemoReadings() {
            BasicReadings = thirteenMonthDemoReadings.ToImmutableList(),
            CostedReadings = thirteenMonthDemoCostedReadings.ToImmutableList()
        };
    }
}
