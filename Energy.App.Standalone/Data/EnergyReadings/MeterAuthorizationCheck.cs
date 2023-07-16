using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.n3rgyApi.Interfaces;
using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings
{
    public class MeterAuthorizationCheck : IMeterAuthorizationCheck
    {
        private readonly IN3rgyEnergyDataService _n3rgyEnergyDataService;

        public MeterAuthorizationCheck(IN3rgyEnergyDataService n3rgyEnergyDataService)
        {
            _n3rgyEnergyDataService = n3rgyEnergyDataService;
        }

        public async Task<TestAccessResponse> TestAccess(MeterType meterType, string mac, CancellationToken ctx = default)
        {
            using HttpResponseMessage response = await _n3rgyEnergyDataService.TestAccess(meterType, mac, ctx);
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
