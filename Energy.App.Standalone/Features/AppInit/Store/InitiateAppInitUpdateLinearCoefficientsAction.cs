namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class InitiateAppInitUpdateLinearCoefficientsAction
    {
        public bool CanUpdateLinearCoefficients { get; }

        public InitiateAppInitUpdateLinearCoefficientsAction(bool canUpdateLinearCoefficients)
        {
            CanUpdateLinearCoefficients = canUpdateLinearCoefficients;
        }
    }
}