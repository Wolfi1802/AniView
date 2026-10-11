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

            string? format = null;

            if (!string.IsNullOrWhiteSpace(media.Format))
            {
                format = FormatLabels.TryGetValue(media.Format, out var label)
                    ? label
                    : media.Format;
            }

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
                NextEpisodeNumber = airing?.Episode,
                NextEpisodeAiringAt = airingAt,
                TimeUntilNextEpisode = timeUntilAiring
            };
        }

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
