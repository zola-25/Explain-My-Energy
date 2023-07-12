using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions
{
    public class NotifyGasLoadingFinished
    {
        
        public bool Updated { get; }
        public string Error { get; }

        public NotifyGasLoadingFinished(bool updated, string error = null)
        {
            Updated = updated;
            Error = error;
        }

        
    }

}