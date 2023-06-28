using Energy.App.Standalone;
using Energy.WeatherReadings;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

System.Reflection.Assembly currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options =>
{
    Fluxor.DependencyInjection.FluxorOptions fluxorOptions = options.ScanAssemblies(currentAssembly);
#if DEBUG
    fluxorOptions.UseReduxDevTools();
#endif
});

builder.Services.AddWeatherDataService();

builder.Services.AddMudServices();


await builder.Build().RunAsync();
