
namespace Energy.App.Standalone.Services.FluxorPersist;

public class DbState : IDbState
{
    public string StateName
    {
        get;
        set;
    }

    public object State
    {
        get;
        set;
    }
}