using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class Co2ConversionFactors
{
    /// <summary>
    /// https://www.carbontrust.com/our-work-and-impact/guides-reports-and-tools/conversion-factors-energy-and-carbon-conversion-guide
    /// </summary>
    public decimal GetCo2ConversionFactor(MeterType meterType)
    {
        return meterType switch
        {
            MeterType.Gas => 0.18254m,
            MeterType.Electricity => 0.19338m,
            _ => throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null)
        };
    }
}
