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
        private static readonly Dictionary<string, string> StatusLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["FINISHED"] = "Beendet",
            ["RELEASING"] = "Läuft aktuell",
            ["NOT_YET_RELEASED"] = "Noch nicht erschienen",
            ["CANCELLED"] = "Abgebrochen",
            ["HIATUS"] = "Pausiert"
        };

        private static readonly Dictionary<string, string> SeasonLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["WINTER"] = "Winter",
            ["SPRING"] = "Frühling",
            ["SUMMER"] = "Sommer",
            ["FALL"] = "Herbst"
        };

        private static readonly Dictionary<string, string> SourceLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ORIGINAL"] = "Original",
            ["MANGA"] = "Manga",
            ["LIGHT_NOVEL"] = "Light Novel",
            ["VISUAL_NOVEL"] = "Visual Novel",
            ["VIDEO_GAME"] = "Videospiel",
            ["OTHER"] = "Sonstiges",
            ["NOVEL"] = "Roman",
            ["DOUJINSHI"] = "Doujinshi",
            ["ANIME"] = "Anime",
            ["WEB_NOVEL"] = "Web Novel",
            ["LIVE_ACTION"] = "Realfilm",
            ["GAME"] = "Spiel",
            ["COMIC"] = "Comic",
            ["MULTIMEDIA_PROJECT"] = "Multimedia-Projekt",
            ["PICTURE_BOOK"] = "Bilderbuch"
        };

        private static readonly Dictionary<string, string> RelationLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ADAPTATION"] = "Vorlage",
            ["PREQUEL"] = "Vorgänger",
            ["SEQUEL"] = "Nachfolger",
            ["PARENT"] = "Hauptgeschichte",
            ["SIDE_STORY"] = "Nebengeschichte",
            ["CHARACTER"] = "Gemeinsame Charaktere",
            ["SUMMARY"] = "Zusammenfassung",
            ["ALTERNATIVE"] = "Alternative",
            ["SPIN_OFF"] = "Spin-off",
            ["OTHER"] = "Sonstiges",
            ["SOURCE"] = "Quelle",
            ["COMPILATION"] = "Kompilation",
            ["CONTAINS"] = "Enthält"
        };

        private static readonly Dictionary<string, string> RoleLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["MAIN"] = "Hauptrolle",
            ["SUPPORTING"] = "Nebenrolle",
            ["BACKGROUND"] = "Hintergrund"
        };

        private static readonly Dictionary<string, string> CountryLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["JP"] = "Japan",
            ["CN"] = "China",
            ["KR"] = "Südkorea",
            ["TW"] = "Taiwan"
        };

        private static readonly Dictionary<string, string> RankingTypeLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["RATED"] = "Bewertung",
            ["POPULAR"] = "Beliebtheit"
        };

        public static AnimeDetails Map(MediaDto media)
        {
            var title = AnimeSummaryMapper.FirstNonEmpty(
                media.Title?.English,
                media.Title?.Romaji,
                media.Title?.UserPreferred,
                media.Title?.Native) ?? "Unbekannter Titel";

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

                Format = AnimeSummaryMapper.GetFormatLabel(media.Format),
                Status = Label(StatusLabels, media.Status),
                Season = FormatSeason(media.Season, media.SeasonYear),
                StartDateText = FormatDate(media.StartDate),
                EndDateText = FormatDate(media.EndDate),
                Episodes = media.Episodes,
                DurationMinutes = media.Duration,
                Source = Label(SourceLabels, media.Source),
                CountryOfOrigin = Label(CountryLabels, media.CountryOfOrigin),
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
                Relations = MapRelations(media.Relations),
                Recommendations = MapRecommendations(media.Recommendations),
                ExternalLinks = MapLinks(media.ExternalLinks),
                StreamingEpisodes = MapStreamingEpisodes(media.StreamingEpisodes),
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
                    TranslateRole(person.Role),
                    AnimeSummaryMapper.FirstNonEmpty(person.Image?.Large, person.Image?.Medium)))
                .ToList();

        private static IReadOnlyList<AnimeRelation> MapRelations(EdgeListDto<MediaRelationEdgeDto>? relations) =>
            (relations?.Edges ?? [])
                .Where(edge => edge?.Node is not null)
                .Select(edge => new AnimeRelation(
                    edge!.Node!.Id,
                    TitleOf(edge.Node.Title),
                    Label(RelationLabels, edge.RelationType),
                    AnimeSummaryMapper.GetFormatLabel(edge.Node.Format) ?? Label(null, edge.Node.Type),
                    Label(StatusLabels, edge.Node.Status),
                    CoverOf(edge.Node.CoverImage)))
                .ToList();

        private static IReadOnlyList<AnimeRecommendation> MapRecommendations(NodeListDto<RecommendationDto>? recommendations) =>
            (recommendations?.Nodes ?? [])
                .Where(node => node?.MediaRecommendation is not null)
                .Select(node => new AnimeRecommendation(
                    node!.MediaRecommendation!.Id,
                    TitleOf(node.MediaRecommendation.Title),
                    AnimeSummaryMapper.GetFormatLabel(node.MediaRecommendation.Format),
                    CoverOf(node.MediaRecommendation.CoverImage),
                    node.Rating))
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

        private static IReadOnlyList<AnimeStreamingEpisode> MapStreamingEpisodes(List<MediaStreamingEpisodeDto?>? episodes) =>
            (episodes ?? [])
                .Where(episode => episode is not null)
                .Select(episode => new AnimeStreamingEpisode(
                    Clean(episode!.Title),
                    Clean(episode.Thumbnail),
                    Clean(episode.Url),
                    Clean(episode.Site)))
                .ToList();

        private static IReadOnlyList<AnimeRanking> MapRankings(List<MediaRankDto?>? rankings) =>
            (rankings ?? [])
                .Where(ranking => ranking?.Rank is not null)
                .Select(ranking => new AnimeRanking(
                    ranking!.Rank!.Value,
                    Label(RankingTypeLabels, ranking.Type),
                    Clean(ranking.Context),
                    ranking.Year,
                    Label(SeasonLabels, ranking.Season),
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

        private static string TitleOf(MediaTitleDto? title) =>
            AnimeSummaryMapper.FirstNonEmpty(title?.English, title?.Romaji, title?.UserPreferred, title?.Native)
            ?? "Unbekannter Titel";

        private static string? CoverOf(MediaCoverDto? cover) =>
            AnimeSummaryMapper.FirstNonEmpty(cover?.Large, cover?.Medium, cover?.ExtraLarge);

        private static string? TranslateRole(string? role) => Label(RoleLabels, role);

        /// <summary>
        /// Übersetzt einen AniList-Wert; unbekannte Werte werden lesbar aufbereitet (DOUBLE_WORD wird zu Double Word).
        /// </summary>
        private static string? Label(Dictionary<string, string>? labels, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (labels is not null && labels.TryGetValue(value, out var label))
            {
                return label;
            }

            var words = value.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

            return string.Join(' ', words);
        }

        private static string? FormatSeason(string? season, int? year)
        {
            var label = Label(SeasonLabels, season);

            return (label, year) switch
            {
                (not null, not null) => $"{label} {year}",
                (not null, null) => label,
                (null, not null) => year.ToString(),
                _ => null
            };
        }

        /// <summary>
        /// Bildet ein unvollständiges Datum so genau ab, wie AniList es kennt (Jahr, Monat und Jahr oder vollständig).
        /// </summary>
        private static string? FormatDate(FuzzyDateDto? date) => date switch
        {
            { Year: { } year, Month: { } month, Day: { } day } => $"{day:00}.{month:00}.{year}",
            { Year: { } year, Month: { } month } => $"{month:00}.{year}",
            { Year: { } year } => year.ToString(),
            _ => null
        };

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
        private static string? CleanDescription(string? description)
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
