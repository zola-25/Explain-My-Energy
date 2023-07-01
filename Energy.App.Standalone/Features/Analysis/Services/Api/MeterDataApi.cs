using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api;

public class MeterDataApi : IMeterDataApi
{
    private readonly HttpClient _httpClient;

    public MeterDataApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MeterData> GetMeterData(Guid globalId)
    {
        var meterChart = await _httpClient.GetFromJsonAsync<MeterData>($"/api/ChartData/Meter/{globalId}/Data");
        return meterChart;
    }

    public async Task<AnalysisData> GetMeterAnalysisData(Guid globalId)
    {
        var meterAnalysis = await _httpClient.GetFromJsonAsync<AnalysisData>($"/api/ChartData/Meter/{globalId}/AnalysisData");
        return meterAnalysis;
    }


}