using Energy.n3rgyApi.Models;
using Energy.Shared;

namespace Energy.n3rgyApi.Interfaces;

public interface IConsumptionDataRetriever
{
    Task<N3RgyConsumptionResponse> GetConsumptionResponse(string mac,
        MeterType meterType,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ctx = default);

    Task TestAccessWithException(MeterType meterType,
        string mac, CancellationToken ctx = default);

    Task<HttpResponseMessage> TestAccess(MeterType meterType,
        string mac, CancellationToken ctx = default);


}