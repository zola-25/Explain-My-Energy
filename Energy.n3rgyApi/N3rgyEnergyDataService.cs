using Energy.n3rgyApi.Interfaces;
using Energy.n3rgyApi.Models;
using Energy.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Energy.n3rgyApi;

public class N3rgyEnergyDataService : IN3rgyEnergyDataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<N3rgyEnergyDataService> _logger;

    public N3rgyEnergyDataService(IHttpClientFactory httpClientFactory, ILogger<N3rgyEnergyDataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetHttpClient(MeterType meterType)
    {
        return meterType switch
        {
            MeterType.Gas => _httpClientFactory.CreateClient("n3rgy-gas"),
            MeterType.Electricity => _httpClientFactory.CreateClient("n3rgy-electricity"),
            _ => throw new NotImplementedException()
        };
    }

    public async Task<N3RgyConsumptionResponse> GetConsumptionResponse(string mac,
        MeterType meterType,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ctx = default)
    {
        HttpClient httpClient = GetHttpClient(meterType);
        httpClient.DefaultRequestHeaders.Add("authorization", mac);

        string path = $"1?start={startDate:yyyyMMdd}&end={endDate:yyyyMMdd}";

        using HttpResponseMessage response = await httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead, ctx);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Path {Path} returned {StatusCode} for {Mac}", path, response.StatusCode, mac);
            return new N3RgyConsumptionResponse();
        }

        response.EnsureSuccessStatusCode();

        await using Stream streamResponse = await httpClient.GetStreamAsync(path, ctx);

        JsonSerializerOptions deserializeOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };
        N3RgyConsumptionResponse getConsumptionResponse = await JsonSerializer.DeserializeAsync<N3RgyConsumptionResponse>(
            streamResponse,
            deserializeOptions,
            cancellationToken: ctx);

        return getConsumptionResponse;
    }



    public async Task TestAccessWithException(MeterType meterType,
        string mac, CancellationToken ctx = default)
    {
        DateTime startDate = DateTime.Today.AddDays(-4);
        DateTime endDate = DateTime.Today.AddDays(-3);
        using HttpClient httpClient = GetHttpClient(meterType);
        httpClient.DefaultRequestHeaders.Add("authorization", mac);

        string path = $"1?start={startDate:yyyyMMdd}&end={endDate:yyyyMMdd}";

        using HttpResponseMessage response = await httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead, ctx).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async Task<HttpResponseMessage> TestAccess(MeterType meterType,
        string mac, CancellationToken ctx = default)
    {
        DateTime startDate = DateTime.Today.AddDays(-4);
        DateTime endDate = DateTime.Today.AddDays(-3);
        using HttpClient httpClient = GetHttpClient(meterType);
        httpClient.DefaultRequestHeaders.Add("authorization", mac);

        string path = $"1?start={startDate:yyyyMMdd}&end={endDate:yyyyMMdd}";

        HttpResponseMessage response = await httpClient.GetAsync(
            path,
            HttpCompletionOption.ResponseHeadersRead, ctx).ConfigureAwait(false);

        return response;

    }
}