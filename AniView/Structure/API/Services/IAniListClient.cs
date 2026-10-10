using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Gemeinsamer Transport für alle Abfragen an die AniList-GraphQL-API.
    /// </summary>
    public interface IAniListClient
    {
        /// <summary>
        /// Sendet eine GraphQL-Abfrage und liefert den Inhalt von "data".
        /// </summary>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<T> SendAsync<T>(string query, object? variables, CancellationToken cancellationToken)
            where T : class;
    }
}
