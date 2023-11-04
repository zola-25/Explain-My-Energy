using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Services;

public class AppStatus
{
    public bool HadAutoRedirect { get; private set; }
    public bool IsDemoMode { get; init; }
    public string DocsUri { get; init; }

    public void SetHadAutoRedirect()
    {
        HadAutoRedirect = true;
    }

    public AppStatus(IConfiguration configuration)
    {
        IsDemoMode = configuration.eIsDemoMode();
        DocsUri = configuration["App:DocsUri"] ?? throw new ArgumentException("DocsUri not found in appsettings.json");
    }
}
