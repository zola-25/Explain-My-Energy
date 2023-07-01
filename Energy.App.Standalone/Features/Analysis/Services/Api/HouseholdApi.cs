using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api;

public class HouseholdApi : IHouseholdApi
{
    private readonly HttpClient _httpClient;

    public HouseholdApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HouseholdDetails> GetHouseholdForUser()
    {
        var householdDetails = await _httpClient.GetFromJsonAsync<HouseholdDetails>($"/api/Household");
        return householdDetails;
    }

    public async Task AddOrUpdateAsync(HouseholdDetails householdDetails)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/Household", householdDetails);
        response.EnsureSuccessStatusCode();
    }
}