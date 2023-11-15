using Blazored.LocalStorage;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo;


public class SetDefaultLocalState : ISetDefaultLocalState
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILocalStorageService _localStorage;
    private readonly ISyncLocalStorageService _syncLocalStorage;

    public SetDefaultLocalState(IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILocalStorageService localStorage,
        ISyncLocalStorageService syncLocalStorage
        )
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _syncLocalStorage = syncLocalStorage;
        IsDemoMode = configuration.eIsDemoMode();
    }

    public bool IsDemoMode { get; private set; }

    public DemoGasReadings DemoGasReadings { get; private set; }
    public DemoElectricityReadings DemoElectricityReadings { get; private set; }
    public DemoMeterSetup DemoMeterSetup { get; private set; }
    public DemoHousehold DemoHousehold { get; private set; }



    public async Task LoadDefaultsIfDemo()  
    {
        if (!IsDemoMode)
        {
            return;
        }

        await _localStorage.ClearAsync();

        var httpClient = _httpClientFactory.CreateClient("DemoData");

        var jsonDemoHousehold = await httpClient.GetFromJsonAsync<DemoHousehold>("demo/household.json");

        DemoHousehold = jsonDemoHousehold;

        var jsonDemoMeterSetup = await httpClient.GetFromJsonAsync<DemoMeterSetup>("demo/meterSetupState.json");

        DemoMeterSetup = jsonDemoMeterSetup;

        var jsonDemoGasReadings = await httpClient.GetFromJsonAsync<DemoGasReadings>("demo/gasReadings.json");
        DemoGasReadings = jsonDemoGasReadings;
        

        var jsonDemoElecReadings = await httpClient.GetFromJsonAsync<DemoElectricityReadings>("demo/electricityReadings.json");

        DemoElectricityReadings = jsonDemoElecReadings;



    }

    public void ClearAllData()
    {
        _syncLocalStorage.Clear();
    }
}
