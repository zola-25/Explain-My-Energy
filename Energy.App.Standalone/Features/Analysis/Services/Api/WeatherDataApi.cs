using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api
{
    public class WeatherDataApi : IWeatherDataApi
    {
        private readonly HttpClient _httpClient;

        public WeatherDataApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<List<DailyWeatherReading>> GetWeatherData(DateTime? from = null)
        {
            string url = from == null ? $"/api/WeatherData" : $"/api/WeatherData?from={from.Value}";
            List<DailyWeatherReading> data = await _httpClient.GetFromJsonAsync<List<DailyWeatherReading>>(url);
            return data;
        }
    }
}
