using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api
{
    public class DefaultTariffApi : IDefaultTariffApi
    {
        private readonly HttpClient _httpClient;

        public DefaultTariffApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DefaultTariffDetail> GetCurrentExampleTariff(MeterType meterType, ExampleTariffType exampleTariffType, CancellationToken ctx = default)
        {
            return await _httpClient.GetFromJsonAsync<DefaultTariffDetail>($"/api/DefaultTariff/CurrentExample?MeterType={meterType}&TariffType={exampleTariffType}", cancellationToken: ctx);
        }

        public async Task<List<DefaultTariffDetail>> GetDefaultTariffs(MeterType meterType, ExampleTariffType exampleTariffType, CancellationToken ctx = default)
        {
            var response =
                await _httpClient.GetFromJsonAsync<List<DefaultTariffDetail>>($"/api/DefaultTariff/{meterType}/{exampleTariffType}", cancellationToken: ctx);

            return response;
        }
    }
}
