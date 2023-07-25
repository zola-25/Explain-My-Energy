namespace Energy.App.Standalone.Pages.SetupWizard
{
    [Flags]
    public enum SetupStageFlags
    {
        None = 0,
        NotSeenWelcomeScreenSplash = 1,
        HouseholdNotSetup = 2,
        GasMeterNotSetup = 4,
        GasMeterNotAuthorized = 8,
        ElectricityMeterNotSetup = 16,
        ElectricityMeterNotAuthorized = 32

    }
}
