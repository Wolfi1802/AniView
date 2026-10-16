namespace AniView.Models
{
    /// <summary>
    /// Spalte beziehungsweise Eigenschaft, nach der eine Anime-Liste sortiert wird.
    /// </summary>
    public enum AnimeSortField
    {
        Popularity,
        Score,
        Title,
        Episodes,
        Format
    }

    /// <summary>
    /// Format eines Animes, nach dem gefiltert werden kann.
    /// </summary>
    public enum AnimeFormat
    {
        Tv,
        TvShort,
        Movie,
        Special,
        Ova,
        Ona,
        Music
    }

    /// <summary>
    /// Sortierung und Filter einer Anime-Liste. Zwei Instanzen mit gleichem Inhalt sind gleich,
    /// deshalb eignet sich das Objekt als Schlüssel, um eine Liste bei Änderungen neu zu laden.
    /// </summary>
    public sealed record AnimeListOptions
    {
        public static AnimeListOptions Default { get; } = new();

        public AnimeSortField SortField { get; init; } = AnimeSortField.Popularity;

        public bool Descending { get; init; } = true;

        /// <summary>
        /// Nur Animes dieses Formats, null für alle Formate.
        /// </summary>
        public AnimeFormat? Format { get; init; }

        /// <summary>
        /// Suchbegriff für den Titel, null oder leer für keine Einschränkung.
        /// </summary>
        public string? Search { get; init; }

        /// <summary>
        /// Gibt an, ob Sortierung und Filter dem Standard entsprechen.
        /// </summary>
        public bool IsDefault => this == Default;

        /// <summary>
        /// Wechselt die Sortierung: bei gleicher Spalte kehrt sich die Richtung um,
        /// bei einer anderen Spalte gilt deren Standardrichtung.
        /// </summary>
        public AnimeListOptions SortedBy(AnimeSortField field) =>
            field == SortField
                ? this with { Descending = !Descending }
                : this with { SortField = field, Descending = GetDefaultDescending(field) };

        /// <summary>
        /// Wählt die Spalte und deren Standardrichtung (Titel aufsteigend, alle anderen absteigend).
        /// </summary>
        public AnimeListOptions WithSortField(AnimeSortField field) =>
            this with { SortField = field, Descending = GetDefaultDescending(field) };

        public AnimeListOptions WithSearch(string? search) =>
            this with { Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim() };

        /// <summary>
        /// Schlüssel nur der Filter (ohne Sortierung), zum Beispiel für das Zwischenspeichern von Gesamtanzahlen.
        /// </summary>
        public string FilterKey => $"{Format?.ToString() ?? "-"}|{Search?.ToLowerInvariant() ?? "-"}";

        /// <summary>
        /// Schlüssel aus Filter und Sortierung, zum Beispiel für das Zwischenspeichern von Seiten.
        /// </summary>
        public string CacheKey => $"{FilterKey}|{SortField}|{(Descending ? "desc" : "asc")}";

        private static bool GetDefaultDescending(AnimeSortField field) => field != AnimeSortField.Title;
    }
}
