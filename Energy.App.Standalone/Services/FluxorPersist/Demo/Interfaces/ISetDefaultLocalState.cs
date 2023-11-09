using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;

public interface ISetDefaultLocalState
{
    DemoGasReadings DemoGasReadings { get;  }
    DemoElectricityReadings DemoElectricityReadings { get; }
    DemoMeterSetup DemoMeterSetup { get; }
    DemoHousehold DemoHousehold { get; }
    bool IsDemoMode { get; }

    void ClearAllData();
    Task LoadDefaultsIfDemo();
}