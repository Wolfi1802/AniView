using AniView.Models;
using AniView.Structure.API.Dtos;

namespace AniView.Structure.API.Mapping
{
    /// <summary>
    /// Überführt die AniList-Transportmodelle in die Modelle der Anwendung.
    /// </summary>
    internal static class AnimeSummaryMapper
    {
        private static readonly Dictionary<string, string> FormatLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["TV"] = "TV",
            ["TV_SHORT"] = "TV-Kurzserie",
            ["MOVIE"] = "Film",
            ["SPECIAL"] = "Special",
            ["OVA"] = "OVA",
            ["ONA"] = "ONA",
            ["MUSIC"] = "Musik"
        };

        public static IReadOnlyList<AnimeSummary> MapAll(MediaPageData data) =>
            (data.Page?.Media ?? [])
                .Where(media => media is not null)
                .Select(media => Map(media!))
                .ToList();

        /// <summary>
        /// Bildet eine Seite inklusive der Seiteninformationen von AniList ab.
        /// </summary>
        /// <param name="data">Antwort der API.</param>
        /// <param name="requestedPerPage">Angefragte Seitengröße, falls die API keine liefert.</param>
        public static PageResult<AnimeSummary> MapPage(MediaPageData data, int requestedPerPage)
        {
            var items = MapAll(data);
            var info = data.Page?.PageInfo;
            var perPage = info?.PerPage ?? requestedPerPage;

            // Ohne Angabe der API gilt eine volle Seite als "es gibt eventuell mehr".
            var hasNextPage = info?.HasNextPage ?? items.Count >= perPage;

            return new PageResult<AnimeSummary>(items, hasNextPage, info?.Total, perPage);
        }

        public static AnimeSummary Map(MediaDto media)
        {
            var title = FirstNonEmpty(
                media.Title?.English,
                media.Title?.Romaji,
                media.Title?.UserPreferred,
                media.Title?.Native) ?? "Unbekannter Titel";

            var cover = FirstNonEmpty(
                media.CoverImage?.ExtraLarge,
                media.CoverImage?.Large,
                media.CoverImage?.Medium);

            var format = GetFormatLabel(media.Format);

            var airing = media.NextAiringEpisode;

            DateTimeOffset? airingAt = airing?.AiringAt is int timestamp
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
                : null;

            TimeSpan? timeUntilAiring = airing?.TimeUntilAiring is int seconds
                ? TimeSpan.FromSeconds(seconds)
                : null;

            return new AnimeSummary(
                media.Id,
                title,
                cover,
                format,
                media.AverageScore,
                media.Popularity)
            {
                IsFinished = string.Equals(media.Status, "FINISHED", StringComparison.OrdinalIgnoreCase),
                Episodes = media.Episodes,
                AiredText = FormatAiredRange(
                    media.StartDate,
                    media.EndDate,
                    string.Equals(media.Status, "FINISHED", StringComparison.OrdinalIgnoreCase)),
                NextEpisodeNumber = airing?.Episode,
                NextEpisodeAiringAt = airingAt,
                TimeUntilNextEpisode = timeUntilAiring
            };
        }

        /// <summary>
        /// Bildet die Zeitspanne der Erscheinung ab. Ohne Enddatum gilt eine laufende Serie als "seit ...",
        /// ein beendeter Anime zeigt nur den Start.
        /// </summary>
        internal static string? FormatAiredRange(FuzzyDateDto? start, FuzzyDateDto? end, bool isFinished)
        {
            var from = FormatMonth(start);
            if (from is null)
            {
                return null;
            }

            var to = FormatMonth(end);
            if (to is null)
            {
                return isFinished ? from : $"seit {from}";
            }

            return to == from ? from : $"{from} – {to}";
        }

        // Monat und Jahr (zum Beispiel 10.2023), bei unbekanntem Monat nur das Jahr.
        private static string? FormatMonth(FuzzyDateDto? date) => date switch
        {
            { Year: { } year, Month: { } month } => $"{month:00}.{year}",
            { Year: { } year } => year.ToString(),
            _ => null
        };

        /// <summary>
        /// Übersetzt das AniList-Format (zum Beispiel MOVIE) in die Anzeige; unbekannte Werte bleiben unverändert.
        /// </summary>
        internal static string? GetFormatLabel(string? format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return null;
            }

            return FormatLabels.TryGetValue(format, out var label) ? label : format;
        }

        internal static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
