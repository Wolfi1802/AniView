using System.Text;

namespace AniView.Structure.API.Querys
{
    /// <summary>
    /// GraphQL-Abfragen für AniList. Wird nur von den Services verwendet, nicht von Komponenten.
    /// Die Feldauswahl wird über ein <see cref="MediaFieldsProfile"/> zusammengesetzt.
    /// </summary>
    internal static class ApiQueries
    {
        /// <summary>
        /// Einfache Felder der ersten Ebene, die immer abgefragt werden.
        /// </summary>
        private const string BaseFields = """
            id
            idMal
            type
            format
            status(version: 2)
            description(asHtml: false)
            season
            seasonYear
            seasonInt
            episodes
            duration
            chapters
            volumes
            countryOfOrigin
            isLicensed
            source(version: 3)
            hashtag
            updatedAt
            bannerImage
            genres
            synonyms
            averageScore
            meanScore
            popularity
            isLocked
            trending
            favourites
            isAdult
            siteUrl
            autoCreateForumThread
            isRecommendationBlocked
            isReviewBlocked
            """;

        // Objekte der zweiten Ebene. Verschachtelte Listen sind begrenzt, damit die
        // Abfrage unter dem Komplexitätslimit der AniList-API bleibt.
        private const string TitleFields = """
            title {
              romaji
              english
              native
              userPreferred
            }
            """;

        private const string StartDateFields = "startDate { year month day }";

        private const string EndDateFields = "endDate { year month day }";

        private const string TrailerFields = "trailer { id site thumbnail }";

        private const string CoverImageFields = """
            coverImage {
              extraLarge
              large
              medium
              color
            }
            """;

        private const string TagsFields = """
            tags {
              id
              name
              description
              category
              rank
              isGeneralSpoiler
              isMediaSpoiler
              isAdult
            }
            """;

        private const string RelationsFields = """
            relations {
              edges {
                id
                relationType(version: 2)
                node {
                  id
                  title { romaji english native userPreferred }
                  type
                  format
                  status(version: 2)
                  coverImage { extraLarge large medium color }
                }
              }
            }
            """;

        private const string CharactersFields = """
            characters(perPage: 10, sort: [ROLE, RELEVANCE, ID]) {
              edges {
                id
                role
                node {
                  id
                  name { full native }
                  image { large medium }
                }
              }
            }
            """;

        private const string StaffFields = """
            staff(perPage: 10, sort: [RELEVANCE, ID]) {
              edges {
                id
                role
                node {
                  id
                  name { full native }
                  image { large medium }
                }
              }
            }
            """;

        private const string StudiosFields = """
            studios {
              edges {
                id
                isMain
                node {
                  id
                  name
                  isAnimationStudio
                  siteUrl
                }
              }
            }
            """;

        private const string NextAiringEpisodeFields = """
            nextAiringEpisode {
              id
              airingAt
              timeUntilAiring
              episode
              mediaId
            }
            """;

        private const string AiringScheduleFields = """
            airingSchedule(perPage: 10, notYetAired: true) {
              nodes {
                id
                airingAt
                timeUntilAiring
                episode
                mediaId
              }
            }
            """;

        private const string TrendsFields = """
            trends(perPage: 10, sort: [DATE_DESC]) {
              nodes {
                mediaId
                date
                trending
                averageScore
                popularity
                inProgress
                releasing
                episode
              }
            }
            """;

        private const string ExternalLinksFields = """
            externalLinks {
              id
              url
              site
              siteId
              type
              language
              color
              icon
              notes
              isDisabled
            }
            """;

        private const string StreamingEpisodesFields = """
            streamingEpisodes {
              title
              thumbnail
              url
              site
            }
            """;

        private const string RankingsFields = """
            rankings {
              id
              rank
              type
              format
              year
              season
              allTime
              context
            }
            """;

        private const string RecommendationsFields = """
            recommendations(perPage: 5, sort: [RATING_DESC]) {
              nodes {
                id
                rating
                mediaRecommendation {
                  id
                  title { romaji english native userPreferred }
                  type
                  format
                  status(version: 2)
                  coverImage { extraLarge large medium color }
                }
              }
            }
            """;

        private const string ReviewsFields = """
            reviews(limit: 5, sort: [RATING_DESC]) {
              nodes {
                id
                summary
                rating
                ratingAmount
                score
                createdAt
                user {
                  id
                  name
                }
              }
            }
            """;

        private const string PageInfoFields = "pageInfo { total perPage currentPage lastPage hasNextPage }";

        public static string MediaPage(MediaFieldsProfile profile) => $$"""
            query (
              $page: Int,
              $perPage: Int,
              $sort: [MediaSort],
              $search: String,
              $formatIn: [MediaFormat]
            ) {
              Page(page: $page, perPage: $perPage) {
                {{PageInfoFields}}
                media(
                  type: ANIME,
                  sort: $sort,
                  search: $search,
                  format_in: $formatIn,
                  isAdult: false
                ) {
                  {{BuildMediaFields(profile)}}
                }
              }
            }
            """;

        public static string CurrentSimulcast(MediaFieldsProfile profile) => $$"""
            query (
              $page: Int,
              $perPage: Int,
              $season: MediaSeason!,
              $seasonYear: Int!,
              $sort: [MediaSort],
              $search: String,
              $formatIn: [MediaFormat]
            ) {
              Page(page: $page, perPage: $perPage) {
                {{PageInfoFields}}
                media(
                  type: ANIME,
                  season: $season,
                  seasonYear: $seasonYear,
                  status: RELEASING,
                  sort: $sort,
                  search: $search,
                  format_in: $formatIn,
                  isAdult: false
                ) {
                  {{BuildMediaFields(profile)}}
                }
              }
            }
            """;

        /// <summary>
        /// Ein einzelner Anime anhand seiner ID, zum Beispiel für die Detailansicht.
        /// </summary>
        public static string MediaById(MediaFieldsProfile profile) => $$"""
            query ($id: Int) {
              Media(id: $id, type: ANIME) {
                {{BuildMediaFields(profile)}}
              }
            }
            """;

        /// <summary>
        /// Nur die IDs der laufenden Animes der Season. Dient dazu, die Gesamtanzahl exakt zu zählen,
        /// weil "pageInfo.total" von AniList eine Schätzung ist.
        /// </summary>
        public const string CurrentSimulcastIds = """
            query (
              $page: Int,
              $perPage: Int,
              $season: MediaSeason!,
              $seasonYear: Int!,
              $search: String,
              $formatIn: [MediaFormat]
            ) {
              Page(page: $page, perPage: $perPage) {
                media(
                  type: ANIME,
                  season: $season,
                  seasonYear: $seasonYear,
                  status: RELEASING,
                  search: $search,
                  format_in: $formatIn,
                  isAdult: false
                ) {
                  id
                }
              }
            }
            """;

        private static string BuildMediaFields(MediaFieldsProfile profile)
        {
            var fields = new StringBuilder(BaseFields);

            AppendIf(fields, profile.Title, TitleFields);
            AppendIf(fields, profile.StartDate, StartDateFields);
            AppendIf(fields, profile.EndDate, EndDateFields);
            AppendIf(fields, profile.Trailer, TrailerFields);
            AppendIf(fields, profile.CoverImage, CoverImageFields);
            AppendIf(fields, profile.Tags, TagsFields);
            AppendIf(fields, profile.Relations, RelationsFields);
            AppendIf(fields, profile.Characters, CharactersFields);
            AppendIf(fields, profile.Staff, StaffFields);
            AppendIf(fields, profile.Studios, StudiosFields);
            AppendIf(fields, profile.NextAiringEpisode, NextAiringEpisodeFields);
            AppendIf(fields, profile.AiringSchedule, AiringScheduleFields);
            AppendIf(fields, profile.Trends, TrendsFields);
            AppendIf(fields, profile.ExternalLinks, ExternalLinksFields);
            AppendIf(fields, profile.StreamingEpisodes, StreamingEpisodesFields);
            AppendIf(fields, profile.Rankings, RankingsFields);
            AppendIf(fields, profile.Recommendations, RecommendationsFields);
            AppendIf(fields, profile.Reviews, ReviewsFields);

            return fields.ToString();
        }

        private static void AppendIf(StringBuilder fields, bool enabled, string block)
        {
            if (enabled)
            {
                fields.Append('\n').Append(block);
            }
        }
    }
}
