using Blazored.LocalStorage;
using Energy.App.Standalone.Data;
using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Energy.Shared;
using Fluxor;
using System.Net.Http;
using System.Net.Http.Json;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo;


public class SetDefaultLocalState : ISetDefaultLocalState
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ILocalStorageService _localStorage;
    private readonly ISyncLocalStorageService _syncLocalStorage;
    private readonly IDemoReadingsUpdater _demoReadingsUpdater;

    public SetDefaultLocalState(IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILocalStorageService localStorage,
        ISyncLocalStorageService syncLocalStorage,
        IDemoReadingsUpdater demoReadingsUpdater)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _syncLocalStorage = syncLocalStorage;
        IsDemoMode = configuration.eIsDemoMode();
        _demoReadingsUpdater = demoReadingsUpdater;
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

        var jsonDemoHousehold = await httpClient.GetFromJsonAsync<DemoHousehold>("demo/household.json");

        HouseholdState = new HouseholdState {
            IhdMacId = jsonDemoHousehold.IhdMacId,
            Invalid = false,
            MoveInDate = jsonDemoHousehold.MoveInDate,
            OutCodeCharacters = jsonDemoHousehold.OutCodeCharacters,
            PrimaryHeatSource = jsonDemoHousehold.PrimaryHeatSource,
            Saved = true
        };

        var jsonDemoMeterSetup = await httpClient.GetFromJsonAsync<DemoMeterSetup>("demo/meterSetupState.json");

        MeterSetupState = new MeterSetupState() {
            ElectricityMeter = new MeterState {
                Authorized = true,
                GlobalId = jsonDemoMeterSetup.ElectricityMeter.GlobalId,
                MeterType = jsonDemoMeterSetup.ElectricityMeter.MeterType,
                Mpxn = jsonDemoMeterSetup.ElectricityMeter.Mpxn,
                InitialSetupValid = true,
                SetupValid = true,
                AuthorizeFailedMessage = null,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Electricity, ExampleTariffType.StandardFixedDaily)
            },
            GasMeter = new MeterState {
                Authorized = true,
                GlobalId = jsonDemoMeterSetup.GasMeter.GlobalId,
                MeterType = jsonDemoMeterSetup.GasMeter.MeterType,
                Mpxn = jsonDemoMeterSetup.GasMeter.Mpxn,
                InitialSetupValid = true,
                SetupValid = true,
                AuthorizeFailedMessage = null,
                TariffDetails = DefaultTariffData.GetDefaultTariffs(MeterType.Gas, ExampleTariffType.StandardFixedDaily)
            },
        };


        var utcNow = DateTime.UtcNow;
        var jsonDemoGasReadings = await httpClient.GetFromJsonAsync<DemoGasReadings>("demo/gasReadings.json");
        var updatedDemoGasReadings = _demoReadingsUpdater.GetUpdatedReadings(utcNow, jsonDemoGasReadings.BasicReadings, jsonDemoGasReadings.CostedReadings, MeterSetupState[MeterType.Gas].TariffDetails);

        GasReadingsState = new GasReadingsState() {
            BasicReadings = updatedDemoGasReadings.BasicReadings,
            CostedReadings = updatedDemoGasReadings.CostedReadings,
            LastCheckedForNewReadings = utcNow,
            Loading = false
        };

        var jsonDemoElecReadings = await httpClient.GetFromJsonAsync<DemoElectricityReadings>("demo/electricityReadings.json");

        var updatedDemoElecReadings = _demoReadingsUpdater.GetUpdatedReadings(utcNow, jsonDemoElecReadings.BasicReadings, jsonDemoElecReadings.CostedReadings, MeterSetupState[MeterType.Electricity].TariffDetails);
        ElectricityReadingsState = new ElectricityReadingsState() {
            BasicReadings = updatedDemoElecReadings.BasicReadings,
            CostedReadings = updatedDemoElecReadings.CostedReadings,
            LastCheckedForNewReadings = utcNow,
            Loading = false,
        };

    }

    public void ClearAllData()
    {
        _syncLocalStorage.Clear();
    }
}
