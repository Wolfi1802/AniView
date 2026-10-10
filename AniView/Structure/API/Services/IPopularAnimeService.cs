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
    }
}
