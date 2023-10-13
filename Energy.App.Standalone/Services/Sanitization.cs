using Ganss.Xss;

namespace Energy.App.Standalone.Services;

public class Sanitization : ISanitization 
{
    private readonly IHtmlSanitizer _htmlSanitizer;

    public Sanitization(IHtmlSanitizer htmlSanitizer) {
        _htmlSanitizer = htmlSanitizer;
    }

    public string Sanitize(string html) {
        return _htmlSanitizer.Sanitize(html);
    }
}
