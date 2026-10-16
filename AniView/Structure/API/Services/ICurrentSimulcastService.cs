using AniView.Models;
using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Fragt die aktuell laufenden Animes der laufenden Season bei AniList ab.
    /// </summary>
    public interface ICurrentSimulcastService
    {
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<IReadOnlyList<AnimeSummary>> GetCurrentSimulcast(int pageIndex, int itemsPerPage, CancellationToken cancellationToken = default);

        /// <summary>
        /// Liefert eine Seite des aktuellen Simulcasts inklusive Gesamtanzahl, Seitengröße und der Angabe, ob weitere Seiten folgen,
        /// zum Beispiel für Infinite Scroll.
        /// </summary>
        /// <param name="page">Seitennummer ab 1.</param>
        /// <param name="pageSize">Elemente pro Seite (wird auf die Grenzen der API begrenzt).</param>
        /// <param name="options">Sortierung und Filter. Die Gesamtanzahl zählt nur Animes, die dem Filter entsprechen.</param>
        /// <exception cref="ApiRateLimitException">Das Anfragelimit von AniList ist erreicht.</exception>
        /// <exception cref="ApiException">Die Anfrage ist fehlgeschlagen.</exception>
        Task<PageResult<AnimeSummary>> GetCurrentSimulcastPageAsync(int page, int pageSize, AnimeListOptions options, CancellationToken cancellationToken = default);
    }
}
