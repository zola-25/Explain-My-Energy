using Blazored.LocalStorage;
using Energy.App.Standalone;
using Energy.App.Standalone.Data.EnergyReadings;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Data.Weather;
using Energy.App.Standalone.Data.Weather.Interfaces;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;
using Energy.App.Standalone.Features.EnergyReadings;
using Energy.App.Standalone.Services;
using Energy.App.Standalone.Services.DocSnippets;
using Energy.App.Standalone.Services.FluxorPersist;
using Energy.App.Standalone.Services.FluxorPersist.Demo;
using Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
using Energy.n3rgyApi;
using Energy.WeatherReadings;
using Fluxor;
using Fluxor.Persist.Middleware;
using Fluxor.Persist.Storage;
using Ganss.Xss;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Polly.Extensions.Http;
using Polly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await AddAppSettings(builder, "appsettings.json");


builder.Services.AddWeatherDataService();
builder.Services.AddN3rgyServices();

builder.Services.AddTransient<IMeterAuthorizationCheck, MeterAuthorizationCheck>();


builder.Services.AddTransient<IForecastGenerator, ForecastGenerator>();
builder.Services.AddTransient<IForecastCoefficientsCreator, ForecastCoefficientsCreator>();
builder.Services.AddTransient<Co2ConversionFactors>();


builder.Services.AddTransient<IHistoricalConsumptionSummarizer, HistoricalConsumptionSummarizer>();
builder.Services.AddTransient<ITempForecastSummarizer, TempForecastSummarizer>();
builder.Services.AddTransient<IHistoricalForecastSummarizer, HistoricalForecastSummarizer>();

builder.Services.AddTransient<ITermDateRanges, TermDateRanges>();
builder.Services.AddTransient<ICostCalculator, CostCalculator>();
builder.Services.AddTransient<ICostedReadingsToDailyAggregator, CostedReadingsToDailyAggregator>();
builder.Services.AddTransient<IForecastReadingsMovingAverage, ForecastReadingsMovingAverage>();
builder.Services.AddTransient<IHistoricalForecastValidation, HistoricalForecastValidation>();

builder.Services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();

builder.Services.AddTransient<IEnergyImportValidation, EnergyImportValidation>();

builder.Services.AddTransient<IWeatherDataService, WeatherDataService>();

builder.Services.AddMudServices(config => {
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 3;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddBlazoredLocalStorage(config => {
    config.JsonSerializerOptions.WriteIndented = false;
}

);
builder.Services.AddScoped<IStringStateStorage, LocalStateStorage>();
builder.Services.AddScoped<IStoreHandler, JsonStoreHandler>();


System.Reflection.Assembly currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options => {
    options = options.ScanAssemblies(currentAssembly);

#if DEBUG
    options.UseReduxDevTools(devToolsOptions => {
        devToolsOptions.Latency = TimeSpan.FromMilliseconds(1000);
        devToolsOptions.UseSystemTextJson();
    });
#endif
    options.UsePersist(persistMiddlewareOptions => {
        persistMiddlewareOptions.UseInclusionApproach();
    });
});


builder.Services.AddLogging(c => {
    c.SetMinimumLevel(LogLevel.Information);

});

builder.Services.AddSingleton<AppStatus>();

builder.Services.AddHttpClient("DemoData", c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddPolicyHandler(GetRetryPolicy());
    
builder.Services.AddHttpClient("DocsSite", c => c.BaseAddress = new Uri(builder.Configuration["App:DocsUri"] ?? throw new ArgumentException("DocsUri not found in appsettings.json")))
    .AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSingleton<DocSnippetsLoader>();

builder.Services.AddScoped<ISetDefaultLocalState, SetDefaultLocalState>();

bool isDemoMode = builder.Configuration.eIsDemoMode();

if (isDemoMode)
{
    builder.Services.AddTransient<IEnergyReadingRetriever, DemoEnergyReadingRetriever>();
}
else
{
    builder.Services.AddTransient<IEnergyReadingRetriever, EnergyReadingRetriever>();
}

builder.Services.AddTransient<IEnergyReadingService, EnergyReadingService>();


var host = builder.Build();

try
{
    var docsContent = host.Services.GetRequiredService<DocSnippetsLoader>();
    await docsContent.PreloadAllSnippets();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the app.");
}

await host.RunAsync();



static async ValueTask AddAppSettings(WebAssemblyHostBuilder builder, string appsettingsLocation)
{
    using var http = new HttpClient() {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),        
    };
    
    using var response = await http.GetAsync(requestUri: appsettingsLocation);
    using var stream = await response.Content.ReadAsStreamAsync();

    builder.Configuration.AddJsonStream(stream);
}

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2));
};
