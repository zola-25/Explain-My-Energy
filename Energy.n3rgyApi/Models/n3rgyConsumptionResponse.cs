using Energy.n3rgyApi.Interfaces;

namespace Energy.n3rgyApi.Models;

public class N3RgyConsumptionResponse : IN3RgyConsumptionResponse
{
    public string Resource { get; set; }
    public string ResponseTimestamp { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string Granularity { get; set; }
    public List<ConsumptionReading> Values { get; set; } = new();
}