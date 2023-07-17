using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions
{
    public class EnsureCoeffsAndHeatingForecastLoaded
    {
        public int SizeOfUpdate { get; }

        public TaskCompletionSource<bool> Completion { get; } 

        public EnsureCoeffsAndHeatingForecastLoaded(
            int sizeOfUpdate,
            TaskCompletionSource<bool> completion = null)
        {
            SizeOfUpdate = sizeOfUpdate;
            Completion = completion;
        }
    }















}
