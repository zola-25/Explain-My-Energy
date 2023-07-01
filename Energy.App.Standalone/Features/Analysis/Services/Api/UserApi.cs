using Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Features.Analysis.Services.Api
{
    public class UserApi : IUserApi
    {
        private readonly HttpClient _httpClient;

        public UserApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<UserDetails> EnsureCreated()
        {
            HttpResponseMessage response = await _httpClient.PutAsync("/api/User/EnsureCreated", null);
            response.EnsureSuccessStatusCode();

            var userDetails = await response.Content.ReadFromJsonAsync<UserDetails>();

            ArgumentNullException.ThrowIfNull(userDetails);

            return userDetails;
        }
    }
}
