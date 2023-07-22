public class InMemoryStateContainer
{
    private bool closedWelcome;

    public bool ClosedWelcome
    {
        get => closedWelcome;
        set
        {
            closedWelcome = value;
            
            NotifyClosedWelcome(value);
        }
    }

    public event Action<bool> OnChange;

    private void NotifyClosedWelcome(bool closed) => OnChange?.Invoke(closed);
}