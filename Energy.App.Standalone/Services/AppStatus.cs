using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Services;

public class AppStatus
{
    public bool HadAutoRedirect {get; private set; }
    public bool IsDemoMode {get;set; }
    
    public void SetHadAutoRedirect()
    {
        HadAutoRedirect = true;
    }

    public AppStatus(IConfiguration configuration)
    {
        IsDemoMode = configuration.eIsDemoMode();
    }
}
