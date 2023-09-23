using Blazored.LocalStorage;
using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Fluxor;
using System.Net.Http;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Services.FluxorPersist;


public class SetDefaultLocalState : ISetDefaultLocalState
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILocalStorageService _localStorage;
    private readonly ISyncLocalStorageService _syncLocalStorage;


    public SetDefaultLocalState(IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILocalStorageService localStorage,
        ISyncLocalStorageService syncLocalStorage)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _syncLocalStorage = syncLocalStorage;
        IsDemoMode = configuration.eIsDemoMode();
    }

    public bool IsDemoMode { get; private set; }

    public HouseholdState HouseholdState { get; private set; }

    public MeterSetupState MeterSetupState { get; private set; }

    public ElectricityReadingsState ElectricityReadingsState { get; private set; }

    public GasReadingsState GasReadingsState { get; private set; }

    public async Task LoadDefaultsIfDemo()
    {
        if (!IsDemoMode)
        {
            return;
        }

        await _localStorage.ClearAsync();

        var httpClient = _httpClientFactory.CreateClient("DemoData");

        var householdState = await httpClient.GetFromJsonAsync<HouseholdState>("demo/household.json");

        HouseholdState = householdState;

        var meterSetupStateData = await httpClient.GetFromJsonAsync<MeterSetupState>("demo/meterSetupState.json");

        MeterSetupState = meterSetupStateData;


        var gasReadingsData = await httpClient.GetFromJsonAsync<GasReadingsState>("demo/gasReadings.json");

        GasReadingsState = gasReadingsData;

        var electricityReadingsData = await httpClient.GetFromJsonAsync<ElectricityReadingsState>("demo/electricityReadings.json");

        ElectricityReadingsState = electricityReadingsData;

    }

    public void ClearAllData()
    {
        _syncLocalStorage.Clear();
    }
}
