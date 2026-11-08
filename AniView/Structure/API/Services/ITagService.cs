using AniView.Models;

namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Liefert die Tags, nach denen Anime-Listen gefiltert werden können (zum Beispiel Shounen, Seinen, Isekai).
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Liefert alle Tags alphabetisch nach Name (A bis Z). Schlägt die Abfrage bei AniList fehl,
        /// wird eine feste Auswahl (die Zielgruppen-Tags) geliefert, damit der Filter nutzbar bleibt.
        /// </summary>
        Task<IReadOnlyList<AnimeTagInfo>> GetTagsAsync(CancellationToken cancellationToken = default);
    }
}
