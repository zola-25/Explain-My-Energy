using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Services;

public class AppConfig
{
    public bool IsDemoMode {get;set; }
    
    public AppConfig(IConfiguration configuration)
    {
        IsDemoMode = configuration.eIsDemoMode();
    }
}
