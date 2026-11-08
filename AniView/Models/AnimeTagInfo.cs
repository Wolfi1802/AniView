namespace AniView.Models
{
    /// <summary>
    /// Tag eines Animes bei AniList, nach dem gefiltert werden kann (zum Beispiel "Shounen" der Kategorie "Demographic").
    /// </summary>
    /// <param name="Name">Name des Tags, wie ihn AniList führt.</param>
    /// <param name="Category">Kategorie des Tags bei AniList (zum Beispiel "Demographic", "Theme-Action"), leer wenn unbekannt.</param>
    public sealed record AnimeTagInfo(string Name, string Category);
}
