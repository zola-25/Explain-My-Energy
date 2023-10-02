using Energy.Shared;

namespace Energy.App.Standalone.Data.EnergyReadings.Interfaces;

public interface IMeterAuthorizationCheck
{
    Task<TestAccessResponse> TestAccess(MeterType meterType, string mac, CancellationToken ctx = default);
}