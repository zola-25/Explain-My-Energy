using Microsoft.Extensions.Logging;

namespace Energy.Shared;


public static class LogEventIds
{


    public readonly static EventId WeatherImportFail = new(10010, $"WeatherReadings.{nameof(WeatherImportFail)}");
    public readonly static EventId WeatherImportBadResponse = new(10011, $"WeatherReadings.{nameof(WeatherImportBadResponse)}");
    public readonly static EventId WeatherImportException = new(10012, $"WeatherReadings.{nameof(WeatherImportException)}");
    public readonly static EventId WeatherImportFailedToUpdateToLatestDates = new(10014, $"WeatherReadings.{nameof(WeatherImportFailedToUpdateToLatestDates)}");

    public readonly static EventId OutCodeVerificationException = new(20010, $"OutCode.{nameof(OutCodeVerificationException)}");

    public readonly static EventId AppRuntimeSnippetFailedToLoad = new(30010, $"DocSnippet.{nameof(AppRuntimeSnippetFailedToLoad)}");
    
    public readonly static EventId AppInitializationErrorLoadingAllSnippets = new(30020, $"DocSnippet.{nameof(AppInitializationErrorLoadingAllSnippets)}");
    public readonly static EventId AppInitializationErrorLoadingSnippetDocPage = new(30021, $"DocSnippet.{nameof(AppInitializationErrorLoadingSnippetDocPage)}");
    public readonly static EventId AppInitializationErrorLoadingIndividualSnippet = new(30022, $"DocSnippet.{nameof(AppInitializationErrorLoadingIndividualSnippet)}");

}
