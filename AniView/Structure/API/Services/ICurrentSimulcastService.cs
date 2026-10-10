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
    }
}
