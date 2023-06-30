using Blazored.LocalStorage;
using Energy.App.Standalone;
using Energy.App.Standalone.Data.EnergyReadings;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
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

builder.Services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions.WriteIndented = false);
builder.Services.AddScoped<IStringStateStorage, LocalStateStorage>();
builder.Services.AddScoped<IStoreHandler, JsonStoreHandler>();


System.Reflection.Assembly currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options =>
{
    Fluxor.DependencyInjection.FluxorOptions fluxorOptions = options.ScanAssemblies(currentAssembly);
    fluxorOptions.UseReduxDevTools();
    fluxorOptions.UsePersist(options =>
    {
        options.UseInclusionApproach();
    });
});

builder.Services.AddWeatherDataService();
builder.Services.AddN3rgyServices();

builder.Services.AddTransient<IMeterAuthorizationCheck, MeterAuthorizationCheck>();
builder.Services.AddTransient<IEnergyReadingImporter, EnergyReadingImporter>();


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


await builder.Build().RunAsync();
