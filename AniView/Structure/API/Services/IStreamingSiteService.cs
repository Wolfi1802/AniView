using AniView.Structure.API.Exceptions;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Liefert die Streaming-Dienste, nach denen Anime-Listen gefiltert werden können.
    /// </summary>
    public interface IStreamingSiteService
    {
        /// <summary>
        /// Liefert die Namen der Streaming-Dienste alphabetisch. Schlägt die Abfrage bei AniList fehl,
        /// wird eine feste Auswahl geliefert, damit der Filter nutzbar bleibt.
        /// </summary>
        Task<IReadOnlyList<string>> GetStreamingSitesAsync(CancellationToken cancellationToken = default);
    }
}
