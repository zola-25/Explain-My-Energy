namespace Energy.App.Standalone.Services.DocSnippets;

public class AllSnippetsFailedInitializationException : Exception
{
    public AllSnippetsFailedInitializationException() : base("Unable to load any snippet during app initialization")
    {
    }
}
