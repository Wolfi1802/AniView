using AniView.Models;
using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Lädt alle Informationen zu einem einzelnen Anime nach, zum Beispiel für die Detailansicht einer Karte.
    /// </summary>
    public interface IAnimeDetailService
    {
        /// <param name="animeId">AniList-ID des Animes.</param>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen oder der Anime wurde nicht gefunden.</exception>
        Task<AnimeDetails> GetDetailsAsync(int animeId, CancellationToken cancellationToken = default);
    }
}
