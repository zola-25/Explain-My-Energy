using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Setup.Household;
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

        public async Task<List<BasicReading>> ImportFromMoveInOrPreviousYear(MeterType meterType, CancellationToken ctx = default)
        {
            var startDate = _householdState.Value.MoveInDate.Value < DateTime.UtcNow.AddYears(-1) ? DateTime.UtcNow.AddYears(-1) : _householdState.Value.MoveInDate.Value;

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

            foreach (ConsumptionReading energyReading in apiResponse.Values)
            {
                decimal consumptionKWh;
                if (meterType == MeterType.Gas)
                {
                    consumptionKWh = energyReading.Value * 38m * 1.02264m / 3.6m;
                }
                else
                {
                    consumptionKWh = energyReading.Value;
                }

                var utcTime = DateTime.ParseExact(energyReading.Timestamp,
                    "yyyy-MM-dd HH:mm",
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.AssumeUniversal);


                yield return new BasicReading()
                {
                    KWh = consumptionKWh,
                    UtcTime = utcTime,
                };
            }

        }

        public async Task<string> Test()
        {
            await Task.Delay(1000);
            return "Test";
        }
    }
}
