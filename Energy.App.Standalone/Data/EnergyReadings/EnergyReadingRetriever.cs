using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.n3rgyApi.Interfaces;
using Energy.n3rgyApi.Models;
using Energy.Shared;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Energy.App.Standalone.Data.EnergyReadings;

public class EnergyReadingRetriever : IEnergyReadingRetriever
{

    private readonly BatchCreator _batchCreator;
    private readonly IN3rgyEnergyDataService _n3RgyEnergyDataService;

    public EnergyReadingRetriever(IN3rgyEnergyDataService n3RgyEnergyDataService)
    {
        _batchCreator = new BatchCreator();
        _n3RgyEnergyDataService = n3RgyEnergyDataService;
    }


    public async Task<List<BasicReading>> GetMeterReadings(DateTime startDate,
        DateTime endDate,
        string macId,
        MeterType meterType)
    {
        IEnumerable<Batch> batches = _batchCreator.GetBatches(startDate, endDate);

        List<BasicReading> results = new List<BasicReading>();
        foreach (Batch batch in batches)
        {
            N3RgyConsumptionResponse response = await _n3RgyEnergyDataService.GetConsumptionResponse(
                macId,
                meterType,
                batch.StartDate,
                batch.EndDate);

            IEnumerable<BasicReading> basicReadings = ConvertToBasicReadings(meterType, response);
            results.AddRange(basicReadings);
        }
        return results;
    }

    private static IEnumerable<BasicReading> ConvertToBasicReadings(MeterType meterType,
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
}
