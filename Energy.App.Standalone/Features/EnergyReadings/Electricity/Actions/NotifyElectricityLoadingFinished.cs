namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions
{
    public class NotifyElectricityLoadingFinished { 

        public bool Updated { get; }
        public string Error { get; }

        public NotifyElectricityLoadingFinished(bool updated, string error = null)
        {
            Updated = updated;
            Error = error;
        }

    }


}
