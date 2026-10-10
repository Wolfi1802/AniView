using AniView.Models;

namespace AniView.Services
{
    /// <summary>
    /// Kapselt alle Abfragen an die AniList-GraphQL-API.
    /// </summary>
    public interface IAniListService
    {
        /// <summary>
        /// Liefert die beliebtesten Animes (nach Anzahl der Nutzerlisten) in absteigender Reihenfolge.
        /// </summary>
        /// <exception cref="AniListRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="AniListException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default);
    }
}
