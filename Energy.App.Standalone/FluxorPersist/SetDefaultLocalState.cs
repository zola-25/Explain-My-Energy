using Blazored.LocalStorage;
using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Fluxor;
using System.Net.Http.Json;

namespace Energy.App.Standalone.FluxorPersist;


public class SetDefaultLocalState
{
    private ILocalStorageService _localStorage { get; set; }
    private ISyncLocalStorageService _syncLocalStorage { get; set; }


    private HttpClient _httpClient {get;set;}

    public SetDefaultLocalState(IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        ILocalStorageService localStorage,
        ISyncLocalStorageService syncLocalStorage)
    {
        _httpClient = httpClientFactory.CreateClient("DemoData");
        _localStorage = localStorage;
        _syncLocalStorage = syncLocalStorage;
        IsDemoSetup = configuration.eIsDemoMode();
    }

    public static bool IsDemoSetup {get; private set; }

    public static HouseholdState HouseholdState {get; private set;}

    public static MeterSetupState MeterSetupState {get; private set;}

    public static ElectricityReadingsState ElectricityReadingsState {get; private set;}

    public static GasReadingsState GasReadingsState {get; private set;}

    public async Task LoadDefaultsIfDemo()
    {
        if (!IsDemoSetup)
        {
            return;
        }

        await _localStorage.ClearAsync();

        var householdState = await _httpClient.GetFromJsonAsync<HouseholdState>("demo/household.json");

        HouseholdState = householdState;

        var meterSetupStateData = await _httpClient.GetFromJsonAsync<MeterSetupState>("demo/meterSetupState.json");

        MeterSetupState = meterSetupStateData;


        var gasReadingsData = await _httpClient.GetFromJsonAsync<GasReadingsState>("demo/gasReadings.json");

        GasReadingsState = gasReadingsData;

        var electricityReadingsData = await _httpClient.GetFromJsonAsync<ElectricityReadingsState>("demo/electricityReadings.json");

        ElectricityReadingsState = electricityReadingsData;

    }

    public void ClearAllData()
    {
        _syncLocalStorage.Clear();
    }
}
