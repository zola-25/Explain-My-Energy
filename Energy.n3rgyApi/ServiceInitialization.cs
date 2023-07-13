using Energy.n3rgyApi.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Energy.n3rgyApi
{
    public static class ServiceInitialization
    {

        public static IServiceCollection AddN3rgyServices(this IServiceCollection services)
        {
            return services.AddN3rgyServices(options =>
            {
                options.ElectricityApi = "https://consumer-api.data.n3rgy.com/electricity/consumption/";
                options.GasApi = "https://consumer-api.data.n3rgy.com/gas/consumption/";

            });
        }

        public static IServiceCollection AddN3rgyServices(this IServiceCollection services, Action<N3rgyOptions> config)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(config));

            N3rgyOptions options = new N3rgyOptions();
            config(options);

            if (string.IsNullOrWhiteSpace(options.GasApi))
                throw new ArgumentException(nameof(options.GasApi));
            if (string.IsNullOrWhiteSpace(options.ElectricityApi))
                throw new ArgumentException(nameof(options.ElectricityApi));


            services.Configure(config);

            services.AddHttpClient("n3rgy-electricity", (s, c) =>
            {
                c.BaseAddress = new Uri(options.ElectricityApi);
            }).AddPolicyHandler(GetRetryPolicy());


            services.AddHttpClient("n3rgy-gas", (s, c) =>
            {
                c.BaseAddress = new Uri(options.GasApi);
            }).AddPolicyHandler(GetRetryPolicy());


            services.AddTransient<IN3rgyEnergyDataService, N3rgyEnergyDataService>();

            // Register other services here if needed
            return services;
        }



        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2));
        }

    }


}
