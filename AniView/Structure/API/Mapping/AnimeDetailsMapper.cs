using AniView.Models;
using AniView.Structure.API.Dtos;
using System.Net;
using System.Text.RegularExpressions;

namespace AniView.Structure.API.Mapping
{
    /// <summary>
    /// Überführt ein vollständig geladenes AniList-Media in <see cref="AnimeDetails"/>.
    /// </summary>
    internal static partial class AnimeDetailsMapper
    {
        public static AnimeDetails Map(MediaDto media)
        {
            var title = AnimeSummaryMapper.FirstNonEmpty(
                media.Title?.English,
                media.Title?.Romaji,
                media.Title?.UserPreferred,
                media.Title?.Native) ?? string.Empty;

            return new AnimeDetails(media.Id, title)
            {
                RomajiTitle = Clean(media.Title?.Romaji),
                EnglishTitle = Clean(media.Title?.English),
                NativeTitle = Clean(media.Title?.Native),
                Synonyms = Texts(media.Synonyms),
                CoverUrl = AnimeSummaryMapper.FirstNonEmpty(
                    media.CoverImage?.ExtraLarge,
                    media.CoverImage?.Large,
                    media.CoverImage?.Medium),
                BannerUrl = Clean(media.BannerImage),
                Description = CleanDescription(media.Description),

                Format = Clean(media.Format),
                Status = Clean(media.Status),
                Season = Clean(media.Season),
                SeasonYear = media.SeasonYear,
                StartDate = MapDate(media.StartDate),
                EndDate = MapDate(media.EndDate),
                Episodes = media.Episodes,
                DurationMinutes = media.Duration,
                Source = Clean(media.Source),
                CountryOfOrigin = Clean(media.CountryOfOrigin),
                IsLicensed = media.IsLicensed,
                Hashtag = Clean(media.Hashtag),
                Genres = Texts(media.Genres),

                AverageScore = media.AverageScore,
                MeanScore = media.MeanScore,
                Popularity = media.Popularity,
                Favourites = media.Favourites,
                Trending = media.Trending,
                MalId = media.IdMal,
                SiteUrl = Clean(media.SiteUrl),
                UpdatedAt = FromUnix(media.UpdatedAt),

                Trailer = MapTrailer(media.Trailer),
                Tags = MapTags(media.Tags),
                Studios = MapStudios(media.Studios),
                Characters = MapPeople(media.Characters?.Edges, edge => (edge.Node?.Name, edge.Node?.Image, edge.Role)),
                Staff = MapPeople(media.Staff?.Edges, edge => (edge.Node?.Name, edge.Node?.Image, edge.Role)),
                ExternalLinks = MapLinks(media.ExternalLinks),
                Rankings = MapRankings(media.Rankings),
                Reviews = MapReviews(media.Reviews),
                Trends = MapTrends(media.Trends),
                NextEpisode = MapAiring(media.NextAiringEpisode),
                UpcomingEpisodes = (media.AiringSchedule?.Nodes ?? [])
                    .Select(MapAiring)
                    .OfType<AnimeAiring>()
                    .ToList()
            };
        }

        private static AnimeTrailer? MapTrailer(MediaTrailerDto? trailer) =>
            string.IsNullOrWhiteSpace(trailer?.Id) || string.IsNullOrWhiteSpace(trailer.Site)
                ? null
                : new AnimeTrailer(trailer.Id, trailer.Site, Clean(trailer.Thumbnail));

        private static IReadOnlyList<AnimeTag> MapTags(List<MediaTagDto?>? tags) =>
            (tags ?? [])
                .Where(tag => !string.IsNullOrWhiteSpace(tag?.Name))
                .OrderByDescending(tag => tag!.Rank ?? 0)
                .Select(tag => new AnimeTag(
                    tag!.Name!,
                    Clean(tag.Description),
                    Clean(tag.Category),
                    tag.Rank,
                    tag.IsGeneralSpoiler == true || tag.IsMediaSpoiler == true))
                .ToList();

        private static IReadOnlyList<AnimeStudio> MapStudios(EdgeListDto<StudioEdgeDto>? studios) =>
            (studios?.Edges ?? [])
                .Where(edge => !string.IsNullOrWhiteSpace(edge?.Node?.Name))
                .OrderByDescending(edge => edge!.IsMain == true)
                .Select(edge => new AnimeStudio(
                    edge!.Node!.Name!,
                    edge.IsMain == true,
                    edge.Node.IsAnimationStudio == true,
                    Clean(edge.Node.SiteUrl)))
                .ToList();

        private static IReadOnlyList<AnimePerson> MapPeople<TEdge>(
            List<TEdge?>? edges,
            Func<TEdge, (PersonNameDto? Name, ImageDto? Image, string? Role)> select)
            where TEdge : class =>
            (edges ?? [])
                .Where(edge => edge is not null)
                .Select(edge => select(edge!))
                .Where(person => !string.IsNullOrWhiteSpace(person.Name?.Full))
                .Select(person => new AnimePerson(
                    person.Name!.Full!,
                    Clean(person.Name.Native),
                    Clean(person.Role),
                    AnimeSummaryMapper.FirstNonEmpty(person.Image?.Large, person.Image?.Medium)))
                .ToList();

        private static IReadOnlyList<AnimeExternalLink> MapLinks(List<MediaExternalLinkDto?>? links) =>
            (links ?? [])
                .Where(link => link is { IsDisabled: not true } && !string.IsNullOrWhiteSpace(link.Url))
                .Select(link => new AnimeExternalLink(
                    Clean(link!.Site) ?? link.Url!,
                    link.Url!,
                    Clean(link.Type),
                    Clean(link.Language),
                    Clean(link.Color),
                    Clean(link.Notes)))
                .ToList();

        private static IReadOnlyList<AnimeRanking> MapRankings(List<MediaRankDto?>? rankings) =>
            (rankings ?? [])
                .Where(ranking => ranking?.Rank is not null)
                .Select(ranking => new AnimeRanking(
                    ranking!.Rank!.Value,
                    Clean(ranking.Type),
                    Clean(ranking.Context),
                    ranking.Year,
                    Clean(ranking.Season),
                    ranking.AllTime == true))
                .ToList();

        private static IReadOnlyList<AnimeReview> MapReviews(NodeListDto<ReviewDto>? reviews) =>
            (reviews?.Nodes ?? [])
                .Where(review => review is not null)
                .Select(review => new AnimeReview(
                    Clean(review!.Summary),
                    review.Score,
                    review.Rating,
                    review.RatingAmount,
                    Clean(review.User?.Name),
                    FromUnix(review.CreatedAt)))
                .ToList();

        private static IReadOnlyList<AnimeTrendPoint> MapTrends(NodeListDto<MediaTrendDto>? trends) =>
            (trends?.Nodes ?? [])
                .Where(trend => trend is not null)
                .Select(trend => new AnimeTrendPoint(
                    FromUnix(trend!.Date),
                    trend.Trending,
                    trend.Popularity,
                    trend.AverageScore,
                    trend.InProgress,
                    trend.Episode))
                .ToList();

        private static AnimeAiring? MapAiring(MediaAiringScheduleDto? airing) =>
            airing?.Episode is { } episode
                ? new AnimeAiring(episode, FromUnix(airing.AiringAt))
                : null;

        /// <summary>
        /// Bildet ein unvollständiges Datum so genau ab, wie AniList es kennt; ohne Jahr gibt es kein Datum.
        /// </summary>
        private static AnimePartialDate? MapDate(FuzzyDateDto? date) =>
            date?.Year is { } year ? new AnimePartialDate(year, date.Month, date.Day) : null;

        private static DateTimeOffset? FromUnix(int? seconds) =>
            seconds is { } value ? DateTimeOffset.FromUnixTimeSeconds(value) : null;

        private static IReadOnlyList<string> Texts(List<string?>? values) =>
            (values ?? [])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .ToList();

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// AniList liefert die Beschreibung auch ohne HTML-Modus mit einzelnen Tags (zum Beispiel br oder i).
        /// Zeilenumbrüche bleiben erhalten, alle anderen Tags werden entfernt, damit nie Markup angezeigt wird.
        /// </summary>
        internal static string? CleanDescription(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            var text = LineBreakRegex().Replace(description, "\n");
            text = HtmlTagRegex().Replace(text, string.Empty);
            text = WebUtility.HtmlDecode(text);

            return Clean(text);
        }

        [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase)]
        private static partial Regex LineBreakRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex HtmlTagRegex();
    }
}
