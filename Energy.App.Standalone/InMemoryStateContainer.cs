namespace Energy.App.Standalone;

public class InMemoryStateContainer
{
    private bool _welcomeTermsAccepted;

    public bool WelcomeTermsAccepted
    {
        get => _welcomeTermsAccepted;
        set
        {
            if (_welcomeTermsAccepted == value)
            {
                return;
            }

            _welcomeTermsAccepted = value;

            NotifyWelcomeTermsAcceptanceChange(value);
        }
    }

    public event Action<bool> OnChange;

    private void NotifyWelcomeTermsAcceptanceChange(bool closed) => OnChange?.Invoke(closed);
}