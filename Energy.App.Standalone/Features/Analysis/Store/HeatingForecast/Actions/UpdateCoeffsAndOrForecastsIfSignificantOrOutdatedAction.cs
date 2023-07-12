using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction
    {
        public MeterType MeterType { get; }
        public int SizeOfUpdate { get; }

        public TaskCompletionSource<bool> Completion { get; } 

        public UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction(
            int sizeOfUpdate,
            MeterType meterType,
            TaskCompletionSource<bool> completion)
        {
            SizeOfUpdate = sizeOfUpdate;
            MeterType = meterType;
            Completion = completion;
        }
    }















}
