namespace AniView.Structure.API.Services
{
    /// <summary>
    /// Liefert die Genres, nach denen Anime-Listen gefiltert werden können.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Liefert die Namen aller Genres alphabetisch. Schlägt die Abfrage bei AniList fehl,
        /// wird eine feste Auswahl geliefert, damit der Filter nutzbar bleibt.
        /// </summary>
        Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default);
    }
}
