using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Meter.Store;

namespace Energy.App.Standalone.Services;

public class AppStatus
{
    public bool IsDemoMode { get; init; }
    public string DocsUri { get; init; }


    public AppStatus(IConfiguration configuration)
    {
        IsDemoMode = configuration.eIsDemoMode();
        DocsUri = configuration["App:DocsUri"] ?? throw new ArgumentException("DocsUri not found in appsettings.json");
    }
}
