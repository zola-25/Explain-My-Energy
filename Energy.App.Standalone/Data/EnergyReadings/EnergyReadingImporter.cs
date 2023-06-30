using System.Globalization;
using System.Runtime.CompilerServices;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.n3rgyApi.Interfaces;
using Energy.n3rgyApi.Models;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class EnergyReadingImporter : IEnergyReadingImporter
    {

        private readonly BatchCreator _batchCreator;
        public readonly IState<HouseholdState> _householdState;
        private readonly IConsumptionDataRetriever _consumptionDataRetriever;

        public EnergyReadingImporter(IState<HouseholdState> householdState,
            IConsumptionDataRetriever consumptionDataRetriever
            )
        {
            _householdState = householdState;
            _consumptionDataRetriever = consumptionDataRetriever;
            _batchCreator = new BatchCreator();
        }

        public async Task<List<BasicReading>> ImportFromMoveIn(MeterType meterType, CancellationToken ctx = default)
        {
            var startDate = _householdState.Value.MoveInDate.Value;
            var macId = _householdState.Value.IhdMacId;

            var meterReadings = await GetMeterReadings(startDate, DateTime.Today, macId, meterType, ctx)
                .ToListAsync(ctx);
            return meterReadings;
        }

        public async Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default)
        {
            var macId = _householdState.Value.IhdMacId;

            var meterReadings = await GetMeterReadings(fromDate, DateTime.Today, macId, meterType, ctx)
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

                var basicReadings = ConvertToBasicReadings(meterType, response);
                foreach (var energyReading in basicReadings)
                {
                    yield return energyReading;
                }
            }
        }

        private IEnumerable<BasicReading> ConvertToBasicReadings(MeterType meterType,
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

                var utcTime = DateTimeOffset.ParseExact(energyReading.Timestamp,
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
