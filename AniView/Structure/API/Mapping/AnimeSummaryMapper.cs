using AniView.Models;
using AniView.Structure.API.Dtos;

namespace AniView.Structure.API.Mapping
{
    /// <summary>
    /// Überführt die AniList-Transportmodelle in die Modelle der Anwendung.
    /// </summary>
    internal static class AnimeSummaryMapper
    {
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
                media.Title?.Native) ?? string.Empty;

            var cover = FirstNonEmpty(
                media.CoverImage?.ExtraLarge,
                media.CoverImage?.Large,
                media.CoverImage?.Medium);

            var format = string.IsNullOrWhiteSpace(media.Format) ? null : media.Format.Trim();

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
                Genres = (media.Genres ?? [])
                    .Where(genre => !string.IsNullOrWhiteSpace(genre))
                    .Select(genre => genre!.Trim())
                    .ToList(),
                Description = AnimeDetailsMapper.CleanDescription(media.Description),
                NextEpisodeNumber = airing?.Episode,
                NextEpisodeAiringAt = airingAt,
                TimeUntilNextEpisode = timeUntilAiring
            };
        }

        internal static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
