using Microsoft.Extensions.Logging;

namespace Energy.Shared;


public static class LogEventIds
{


    public readonly static EventId WeatherImportFail = new EventId(10010, $"WeatherReadings.{nameof(WeatherImportFail)}");
    public readonly static EventId WeatherImportBadResponse = new EventId(10011, $"WeatherReadings.{nameof(WeatherImportBadResponse)}");
    public readonly static EventId WeatherImportException = new EventId(10012, $"WeatherReadings.{nameof(WeatherImportException)}");
    public readonly static EventId WeatherImportFailedToUpdateToLatestDates = new EventId(10014, $"WeatherReadings.{nameof(WeatherImportFailedToUpdateToLatestDates)}");

    public readonly static EventId OutCodeVerificationException = new EventId(20010, $"OutCode.{nameof(OutCodeVerificationException)}");

    public readonly static EventId DocumentSnippetFailedToLoad = new EventId(30010, $"DocSnippet.{nameof(DocumentSnippetFailedToLoad)}");
}
