namespace Energy.App.Standalone.PageComponents
{
    [Flags]
    public enum SetupStage
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
