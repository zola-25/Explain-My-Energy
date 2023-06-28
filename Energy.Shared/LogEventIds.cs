using Microsoft.Extensions.Logging;

namespace Energy.Shared
{

    public static class LogEventIds
    {


        public static EventId WeatherImportFail = new EventId(10010, $"Portal.{nameof(WeatherImportFail)}");
        public static EventId WeatherImportBadResponse = new EventId(10011, $"Portal.{nameof(WeatherImportBadResponse)}");
        public static EventId WeatherImportException = new EventId(10012, $"Portal.{nameof(WeatherImportException)}");
        public static EventId WeatherImportFailedToUpdateToLatestDates = new EventId(10014, $"Portal.{nameof(WeatherImportFailedToUpdateToLatestDates)}");

        public static EventId OutCodeVerificationException = new EventId(20010, $"Portal.{nameof(OutCodeVerificationException)}");
    }
}
