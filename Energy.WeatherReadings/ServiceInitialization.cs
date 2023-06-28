using Energy.WeatherReadings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Energy.WeatherReadings
{
    public static class ServiceInitialization
    {

        public static IServiceCollection AddWeatherDataService(this IServiceCollection services)
        {
            return AddWeatherDataService(services, options =>
            {
                options.PostcodeLocationApi = "https://api.postcodes.io/";
                options.ForecastApi = "https://api.open-meteo.com/v1/";
                options.ClimateApi = "https://climate-api.open-meteo.com/v1/";
                options.HistoricalApi = "https://archive-api.open-meteo.com/v1/";
            });
        }

        public static IServiceCollection AddWeatherDataService(this IServiceCollection services, Action<WeatherDataOptions> config)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(config));

            WeatherDataOptions options = new WeatherDataOptions();
            config(options);

            if (String.IsNullOrWhiteSpace(options.HistoricalApi)) throw new ArgumentException(nameof(options.HistoricalApi));
            if (String.IsNullOrWhiteSpace(options.ForecastApi)) throw new ArgumentException(nameof(options.ForecastApi));
            if (String.IsNullOrWhiteSpace(options.ClimateApi)) throw new ArgumentException(nameof(options.ClimateApi));
            if (String.IsNullOrWhiteSpace(options.PostcodeLocationApi)) throw new ArgumentException(nameof(options.PostcodeLocationApi));


            services.Configure(config);

            services.AddTransient<IWeatherApiCalls, WeatherApiCalls>();
            services.AddTransient<IWeatherDataService, WeatherDataService>();
            services.AddTransient<IOutCodeLocationLookup, OutCodeLocationLookup>();

            services.AddHttpClient("Historical",
                    (serviceProvider, client) =>
                    {
                        client.BaseAddress = new Uri(options.HistoricalApi);
                    })
                .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("Forecast",
                    (serviceProvider, client) =>
                    {
                        client.BaseAddress = new Uri(options.ForecastApi);
                    })
                .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("Climate",
                    (serviceProvider, client) =>
                    {
                        client.BaseAddress = new Uri(options.ClimateApi);
                    })
                .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("PostcodeLocationLookup",
                    (serviceProvider, client) =>
                    {
                        client.BaseAddress = new Uri(options.PostcodeLocationApi);
                    })
                .AddPolicyHandler(GetRetryPolicy());

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
