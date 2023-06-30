using Energy.n3rgyApi.Models;

namespace Energy.n3rgyApi.Interfaces;

public interface IN3RgyConsumptionResponse
{
    string Resource { get; set; }
    string ResponseTimestamp { get; set; }
    string Start { get; set; }
    string End { get; set; }
    string Granularity { get; set; }
    List<ConsumptionReading> Values { get; set; }
}