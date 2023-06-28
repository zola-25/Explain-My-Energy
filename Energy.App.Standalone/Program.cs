using Energy.App.Standalone;
using Energy.WeatherReadings;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var currentAssembly = typeof(Program).Assembly;
builder.Services.AddFluxor(options =>
{
    var fluxorOptions = options.ScanAssemblies(currentAssembly);
    #if DEBUG 
        fluxorOptions.UseReduxDevTools();
    #endif
});

builder.Services.AddWeatherDataService();

builder.Services.AddMudServices();


await builder.Build().RunAsync();
