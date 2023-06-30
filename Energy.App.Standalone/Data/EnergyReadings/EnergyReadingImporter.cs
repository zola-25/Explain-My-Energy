using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.n3rgyApi.Interfaces;
using Energy.n3rgyApi.Models;
using Energy.Shared;
using Fluxor;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class EnergyReadingImporter : IEnergyReadingImporter
    {

        private readonly BatchCreator _batchCreator;
        private readonly IState<HouseholdState> _householdState;
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
            DateTime startDate = _householdState.Value.MoveInDate.Value;
            string macId = _householdState.Value.IhdMacId;

            List<BasicReading> meterReadings = await GetMeterReadings(startDate, DateTime.Today, macId, meterType, ctx)
                .ToListAsync(ctx);
            return meterReadings;
        }

        public async Task<List<BasicReading>> ImportFromDate(MeterType meterType, DateTime fromDate, CancellationToken ctx = default)
        {
            string macId = _householdState.Value.IhdMacId;

            List<BasicReading> meterReadings = await GetMeterReadings(fromDate, DateTime.Today, macId, meterType, ctx)
                .ToListAsync(ctx);
            return meterReadings;
        }

        private async IAsyncEnumerable<BasicReading> GetMeterReadings(DateTime startDate,
            DateTime endDate,
            string macId,
            MeterType meterType,
            [EnumeratorCancellation] CancellationToken ctx = default)
        {
            IEnumerable<Batch> batches = _batchCreator.GetBatches(startDate, endDate);

            foreach (Batch batch in batches)
            {
                N3RgyConsumptionResponse response = await _consumptionDataRetriever.GetConsumptionResponse(
                    macId,
                    meterType,
                    batch.StartDate,
                    batch.EndDate, ctx);

                IEnumerable<BasicReading> basicReadings = ConvertToBasicReadings(meterType, response);
                foreach (BasicReading energyReading in basicReadings)
                {
                    yield return energyReading;
                }
            }
        }

        private IEnumerable<BasicReading> ConvertToBasicReadings(MeterType meterType,
            N3RgyConsumptionResponse apiResponse)
        {
            TimeZoneInfo ukTimezone = AppDefaults.GetUkTimezone();

            foreach (ConsumptionReading energyReading in apiResponse.Values)
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

                DateTimeOffset localDateTimeOffset = TimeZoneInfo.ConvertTime(utcTime, ukTimezone);

                yield return new BasicReading()
                {
                    KWh = consumptionKWh,
                    LocalTime = localDateTimeOffset.LocalDateTime,
                };
            }

        }
    }
}
