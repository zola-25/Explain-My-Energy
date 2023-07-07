namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class InitiateAppInitUpdateGasReadingsAction
    {
        public bool CanUpdateGasReadingsData { get; }

        public InitiateAppInitUpdateGasReadingsAction(bool canUpdateGasReadingsData)
        {
            CanUpdateGasReadingsData = canUpdateGasReadingsData;
        }
    }
}
