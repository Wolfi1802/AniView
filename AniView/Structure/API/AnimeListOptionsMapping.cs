using AniView.Models;

namespace AniView.Structure.API
{
    /// <summary>
    /// Überführt <see cref="AnimeListOptions"/> in die Variablen der AniList-Abfragen.
    /// </summary>
    internal static class AnimeListOptionsMapping
    {
        /// <summary>
        /// Erzeugt die Variablen für Seite, Seitengröße, Sortierung und Filter. Nicht gesetzte Filter werden weggelassen.
        /// Weitere Variablen (zum Beispiel Season) können dem Ergebnis hinzugefügt werden.
        /// </summary>
        public static Dictionary<string, object?> ToVariables(this AnimeListOptions options, int page, int perPage)
        {
            var variables = options.ToFilterVariables();
            variables["page"] = page;
            variables["perPage"] = perPage;
            variables["sort"] = options.ToApiSort();

            return variables;
        }

        /// <summary>
        /// Erzeugt nur die Filtervariablen, zum Beispiel für das Zählen der Treffer.
        /// </summary>
        public static Dictionary<string, object?> ToFilterVariables(this AnimeListOptions options)
        {
            var variables = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                variables["search"] = options.Search;
            }

            if (options.Format is { } format)
            {
                variables["formatIn"] = new[] { ToApiFormat(format) };
            }

            if (!string.IsNullOrWhiteSpace(options.StreamingSite))
            {
                // Seiten mit Streaming- oder Lese-Lizenz für den Anime (AniList-Argument "licensedBy").
                variables["licensedBy"] = options.StreamingSite;
            }

            return variables;
        }

        /// <summary>
        /// Liefert die AniList-Sortierung. Als zweites Kriterium dient ein eindeutiger Wert, damit die Reihenfolge
        /// bei gleichen Werten (zum Beispiel gleiches Format) stabil bleibt und beim seitenweisen Laden
        /// keine Einträge doppelt oder gar nicht erscheinen.
        /// </summary>
        public static string[] ToApiSort(this AnimeListOptions options)
        {
            var suffix = options.Descending ? "_DESC" : string.Empty;

            return options.SortField switch
            {
                AnimeSortField.Popularity => [$"POPULARITY{suffix}", "ID"],
                AnimeSortField.Score => [$"SCORE{suffix}", "POPULARITY_DESC", "ID"],
                AnimeSortField.Title => [$"TITLE_ROMAJI{suffix}", "ID"],
                AnimeSortField.Episodes => [$"EPISODES{suffix}", "POPULARITY_DESC", "ID"],
                AnimeSortField.Format => [$"FORMAT{suffix}", "POPULARITY_DESC", "ID"],
                // AniList kann nicht nach der nächsten Episode sortieren. Diese Sortierung übernimmt der Service,
                // die API liefert nur eine stabile Reihenfolge.
                AnimeSortField.NextEpisode => ["POPULARITY_DESC", "ID"],
                _ => ["POPULARITY_DESC", "ID"]
            };
        }

        /// <summary>
        /// AniList-Wert eines Formats (zum Beispiel TV_SHORT).
        /// </summary>
        public static string ToApiFormat(AnimeFormat format) => format switch
        {
            AnimeFormat.Tv => "TV",
            AnimeFormat.TvShort => "TV_SHORT",
            AnimeFormat.Movie => "MOVIE",
            AnimeFormat.Special => "SPECIAL",
            AnimeFormat.Ova => "OVA",
            AnimeFormat.Ona => "ONA",
            AnimeFormat.Music => "MUSIC",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unbekanntes Format.")
        };
    }
}
