using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.n3rgyApi.Interfaces;
using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class MeterAuthorizationCheck : IMeterAuthorizationCheck
    {
        IConsumptionDataRetriever _consumptionDataRetriever;

        public MeterAuthorizationCheck(IConsumptionDataRetriever consumptionDataRetriever)
        {
            _consumptionDataRetriever = consumptionDataRetriever;
        }

        public async Task<TestAccessResponse> TestAccess(MeterType meterType, string mac, CancellationToken ctx = default)
        {
            using HttpResponseMessage response = await _consumptionDataRetriever.TestAccess(meterType, mac, ctx);
            if (response.IsSuccessStatusCode)
            {
                return new TestAccessResponse()
                {
                    Success = true
                };
            }

            return new TestAccessResponse()
            {
                Success = false,
                FailureReason = response.ReasonPhrase
            };
        }
    }
}
