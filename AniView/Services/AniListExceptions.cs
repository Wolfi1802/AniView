namespace AniView.Services
{
    /// <summary>
    /// Allgemeiner Fehler bei der Kommunikation mit der AniList-API.
    /// </summary>
    public class AniListException : Exception
    {
        public AniListException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// AniList hat die Anfrage wegen des Rate-Limits abgelehnt (HTTP 429).
    /// </summary>
    public sealed class AniListRateLimitException : AniListException
    {
        public AniListRateLimitException(TimeSpan? retryAfter)
            : base("Das Anfragelimit von AniList ist erreicht.")
        {
            RetryAfter = retryAfter;
        }

        /// <summary>
        /// Empfohlene Wartezeit laut Retry-After-Header, falls vorhanden.
        /// </summary>
        public TimeSpan? RetryAfter { get; }
    }
}
