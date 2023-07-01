using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading
{
    public class EnergyReadingImporter : IEnergyReadingImporter
    {

        private readonly BatchCreator _batchCreator;
        private readonly UserState _appState;
        private readonly IConsumptionDataRetriever _consumptionDataRetriever;

        public EnergyReadingImporter(UserState appState,
            IConsumptionDataRetriever consumptionDataRetriever
            )
        {
            _appState = appState;
            _consumptionDataRetriever = consumptionDataRetriever;
            _batchCreator = new BatchCreator();
        }

        public async Task<List<BasicReading>> ImportFromMoveIn(Meter meter, CancellationToken ctx = default)
        {
            var startDate = _appState.HouseholdDetails.MoveInDate.Value;
            var macId = _appState.HouseholdDetails.IhdMacId;

            var meterReadings = await GetMeterReadings(startDate, DateTime.Today, macId, meter.MeterType, ctx)
                .ToListAsync(ctx);
            return meterReadings;
        }

        public async Task<List<BasicReading>> ImportFromDate(Meter meter, DateTime fromDate, CancellationToken ctx = default)
        {
            var macId = _appState.HouseholdDetails.IhdMacId;

            var meterReadings = await GetMeterReadings(fromDate, DateTime.Today, macId, meter.MeterType, ctx)
                .ToListAsync(ctx);
            return meterReadings;
        }

        private async IAsyncEnumerable<BasicReading> GetMeterReadings(DateTime startDate,
            DateTime endDate,
            string macId,
            MeterType meterType,
            [EnumeratorCancellation] CancellationToken ctx = default)
        {
            var batches = _batchCreator.GetBatches(startDate, endDate);

            foreach (var batch in batches)
            {
                var response = await _consumptionDataRetriever.GetConsumptionResponse(
                    macId,
                    meterType,
                    batch.StartDate,
                    batch.EndDate, ctx);

                IEnumerable<BasicReading> energyReadings = ConvertToEnergyReadings(meterType, response);
                foreach (BasicReading energyReading in energyReadings)
                {
                    yield return energyReading;
                }
            }
        }

        private IEnumerable<BasicReading> ConvertToEnergyReadings(MeterType meterType,
            N3RgyConsumptionResponse apiResponse)
        {
            var ukTimezone = AppDefaults.GetUkTimezone();

            foreach (var energyReading in apiResponse.Values)
            {
                double consumptionKWh;
                if (meterType == MeterType.Gas)
                {
                    consumptionKWh = energyReading.Value * 38 * 1.02264 / 3.6;
                }
                else
                {
                    consumptionKWh = energyReading.Value;
                }

                DateTimeOffset utcTime = DateTimeOffset.ParseExact(energyReading.Timestamp,
                    "yyyy-MM-dd HH:mm",
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.AssumeUniversal);

                var localDateTimeOffset = TimeZoneInfo.ConvertTime(utcTime, ukTimezone);

                yield return new BasicReading()
                {
                    KWh = consumptionKWh,
                    LocalTime = localDateTimeOffset.LocalDateTime,
                };
            }

        }
    }
}
