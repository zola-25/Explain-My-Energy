namespace Energy.App.Standalone.Services.DocSnippets;

public record SnippetLoadResult
{
    public bool Error { get; init; }
    public string Content { get; init; }
    public string DocPath { get; init; }
}
