namespace AniView.Structure.API.Exceptions
{
    /// <summary>
    /// Allgemeiner Fehler bei der Kommunikation mit der AniList-API.
    /// </summary>
    public class ApiException : Exception
    {
        public ApiException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// AniList hat die Anfrage wegen des Rate-Limits abgelehnt (HTTP 429).
    /// </summary>
    public sealed class ApiRateLimitException : ApiException
    {
        public ApiRateLimitException(TimeSpan? retryAfter)
            : base("Das Anfragelimit von Api ist erreicht.")
        {
            RetryAfter = retryAfter;
        }

        /// <summary>
        /// Empfohlene Wartezeit laut Retry-After-Header, falls vorhanden.
        /// </summary>
        public TimeSpan? RetryAfter { get; }
    }
}
