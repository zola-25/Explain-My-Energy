using Blazored.LocalStorage;
using Energy.App.Standalone;
using Energy.App.Standalone.Data.EnergyReadings;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features;
using Energy.App.Standalone.Features.Analysis.Services.Analysis;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.AppInit.Store.OldAppInit;
using Energy.App.Standalone.FluxorPersist;
using Energy.n3rgyApi;
using Energy.WeatherReadings;
using Fluxor;
using Fluxor.Persist.Middleware;
using Fluxor.Persist.Storage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



builder.Services.AddWeatherDataService();
builder.Services.AddN3rgyServices();

builder.Services.AddTransient<IMeterAuthorizationCheck, MeterAuthorizationCheck>();
builder.Services.AddTransient<IEnergyReadingImporter, EnergyReadingImporter>();

builder.Services.AddTransient<IForecastGenerator, ForecastGenerator>();
builder.Services.AddTransient<IForecastCoefficientsCreator, ForecastCoefficientsCreator>();
builder.Services.AddTransient<Co2ConversionFactors>();


builder.Services.AddTransient<IHistoricalDurationAnalyzer, HistoricalDurationAnalyzer>();
builder.Services.AddTransient<ITempForecastAnalyzer, TempForecastAnalyzer>();
builder.Services.AddTransient<ITermDateRanges, TermDateRanges>();
builder.Services.AddTransient<ICostCalculator, CostCalculator>();



builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddBlazoredLocalStorage(config =>
    {
        config.JsonSerializerOptions.WriteIndented = false;
    }
);
builder.Services.AddScoped<IStringStateStorage, LocalStateStorage>();
builder.Services.AddScoped<IStoreHandler, JsonStoreHandler>();

builder.Services.AddScoped<AppInit>();


System.Reflection.Assembly currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options =>
{
    options = options.ScanAssemblies(currentAssembly);

    options.UseReduxDevTools(devToolsOptions =>
    {
        devToolsOptions.Latency = TimeSpan.FromMilliseconds(1000);
        devToolsOptions.UseSystemTextJson();
        //devToolsOptions.EnableStackTrace();
    });
    options.UsePersist(persistMiddlewareOptions =>
    {
        persistMiddlewareOptions.UseInclusionApproach();
    });
});

await builder.Build().RunAsync();
