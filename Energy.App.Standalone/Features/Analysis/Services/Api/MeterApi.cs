using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api;

public class MeterApi : IMeterApi
{
    private readonly HttpClient _httpClient;

    public MeterApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Meter> AddMeterAsync(Meter meter)
    {
        if (meter.GlobalId != default)
        {
            throw new ArgumentException("Meter already exists");
        }

        var httpResponseMessage = await _httpClient.PostAsJsonAsync("/api/Meter", meter);
        httpResponseMessage.EnsureSuccessStatusCode();

        var newMeter = await httpResponseMessage.Content.ReadFromJsonAsync<Meter>();

        return newMeter;
    }

    public async Task<Meter> GetMeterAsync(Guid globalId)
    {
        var meter = await _httpClient.GetFromJsonAsync<Meter>($"/api/Meter/{globalId}");
        return meter;
    }

    public async Task RemoveMeterAsync(Guid globalId)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"/api/Meter/{globalId}");

        httpResponse.EnsureSuccessStatusCode();

    }


    public async Task<Meter> UpdateTariffDetails(Guid globalId, List<TariffDetail> tariffDetails)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync($"/api/Meter/{globalId}/TariffDetails", tariffDetails);
        httpResponse.EnsureSuccessStatusCode();

        var updatedMeter = await httpResponse.Content.ReadFromJsonAsync<Meter>();
        return updatedMeter;
    }



    public async Task<Meter> AddOrUpdateTariff(Guid globalId, TariffDetail tariffDetail)
    {
        var httpResponse = await _httpClient.PutAsJsonAsync($"/api/Meter/{globalId}/Tariff", tariffDetail);
        httpResponse.EnsureSuccessStatusCode();

        var updatedMeter = await httpResponse.Content.ReadFromJsonAsync<Meter>();
        return updatedMeter;
    }

    public async Task<TestAccessResponse> CheckAuthorization(MeterType meterType)
    {
        var response =
            await _httpClient.GetFromJsonAsync<TestAccessResponse>($"/api/Meter/{(int)meterType}/TestAccess");

        return response;
    }

    public async Task<Meter> Authorize(Guid globalId)
    {
        HttpResponseMessage response = await _httpClient.PutAsync($"/api/Meter/{globalId}/Authorize", null);
        response.EnsureSuccessStatusCode();

        var meter = await response.Content.ReadFromJsonAsync<Meter>();
        return meter;
    }

    public async Task<Meter> StartImport(Guid globalId)
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"/api/ImportData/Meter/{globalId}", null);
        response.EnsureSuccessStatusCode();

        var meter = await response.Content.ReadFromJsonAsync<Meter>();
        return meter;
    }

    public async Task<Dictionary<MeterType, Meter>> GetAllMetersAsync(CancellationToken ctx = default)
    {
        Dictionary<MeterType, Meter> meters = await _httpClient.GetFromJsonAsync<Dictionary<MeterType, Meter>>($"/api/Meter", ctx);

        if (meters.Count > 2)
        {
            throw new ArgumentException("Max 2 meters expected");
        }

        return meters;
    }
}

