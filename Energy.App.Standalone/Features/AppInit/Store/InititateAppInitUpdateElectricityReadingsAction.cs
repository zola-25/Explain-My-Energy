namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class InititateAppInitUpdateElectricityReadingsAction
    {
        public bool CanUpdateElectricityReadingsData { get; }

        public InititateAppInitUpdateElectricityReadingsAction(bool canUpdateElectricityReadingsData)
        {
            CanUpdateElectricityReadingsData = canUpdateElectricityReadingsData;
        }
    }
}