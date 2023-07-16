using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction
    {
        public int SizeOfUpdate { get; }

        public TaskCompletionSource<bool> Completion { get; } 

        public UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction(
            int sizeOfUpdate,
            TaskCompletionSource<bool> completion)
        {
            SizeOfUpdate = sizeOfUpdate;
            Completion = completion;
        }
    }















}
