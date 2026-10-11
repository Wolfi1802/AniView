using AniView.Models;
using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Fragt die beliebtesten Animes bei AniList ab.
    /// </summary>
    public interface IPopularAnimeService
    {
        /// <summary>
        /// Liefert die beliebtesten Animes (nach Anzahl der Nutzerlisten) in absteigender Reihenfolge.
        /// </summary>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Liefert eine Seite der beliebtesten Animes (absteigend), zum Beispiel für Infinite Scroll.
        /// </summary>
        /// <param name="page">Seitennummer ab 1.</param>
        /// <param name="pageSize">Elemente pro Seite (wird auf die Grenzen der API begrenzt).</param>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<PageResult<AnimeSummary>> GetPopularPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
