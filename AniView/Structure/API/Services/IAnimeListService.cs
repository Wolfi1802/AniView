using AniView.Models;
using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Kapselt alle Abfragen an die AniList-GraphQL-API.
    /// </summary>
    public interface IAnimeListService
    {
        /// <summary>
        /// Liefert die beliebtesten Animes (nach Anzahl der Nutzerlisten) in absteigender Reihenfolge.
        /// </summary>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default);


        Task<IReadOnlyList<AnimeSummary>> GetCurrentSimulcast(int pageIndex, int itemsPerPage, CancellationToken cancellationToken = default);
    }
}
