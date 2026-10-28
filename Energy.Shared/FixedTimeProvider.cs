namespace Energy.Shared;

/// <summary>
/// A <see cref="TimeProvider"/> whose current time never advances.
/// Used to make date-dependent analysis repeatable, e.g. for testing.
/// Timers and timestamps are left to the system implementation.
/// </summary>
public class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}
