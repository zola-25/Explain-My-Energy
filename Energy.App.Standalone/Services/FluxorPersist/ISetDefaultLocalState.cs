using Energy.App.Standalone.Features.EnergyReadings.Electricity;
using Energy.App.Standalone.Features.EnergyReadings.Gas;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;

namespace Energy.App.Standalone.Services.FluxorPersist;

public interface ISetDefaultLocalState
{
    GasReadingsState GasReadingsState { get; }
    ElectricityReadingsState ElectricityReadingsState { get; }
    MeterSetupState MeterSetupState { get; }
    HouseholdState HouseholdState { get; }
    bool IsDemoMode { get; }

    void ClearAllData();
    Task LoadDefaultsIfDemo();
}