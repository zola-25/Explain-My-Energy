using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api;

public class PostCodeApi : IPostCodeApi
{
    private readonly HttpClient _httpClient;

    public PostCodeApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<OutCode>> SearchOutCodes(string firstLetters, CancellationToken ctx = default)
    {
        List<OutCode> list = await _httpClient.GetFromJsonAsync<List<OutCode>>($"/api/OutCode?filter={firstLetters}", cancellationToken: ctx);
        return list;
    }
}