using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction
    {
        public MeterType MeterType { get; }
        public int SizeOfUpdate { get; }

        public UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction(int sizeOfUpdate, MeterType meterType)
        {
            SizeOfUpdate = sizeOfUpdate;
            MeterType = meterType;
        }
    }















}
