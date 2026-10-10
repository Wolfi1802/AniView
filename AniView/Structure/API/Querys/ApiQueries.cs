namespace AniView.Structure.API.Querys
{
    /// <summary>
    /// GraphQL-Abfragen für AniList. Wird nur vom Service verwendet, nicht von Komponenten.
    /// </summary>
    internal static class ApiQueries
    {
        /// <summary>
        /// Gemeinsame Feldauswahl für alle Media-Abfragen, passend zu MediaDto.
        /// Verschachtelte Listen sind begrenzt, damit die Abfrage unter dem
        /// Komplexitätslimit der AniList-API bleibt.
        /// </summary>
        private const string MediaFields = """
            id
            idMal
            title {
              romaji
              english
              native
              userPreferred
            }
            type
            format
            status(version: 2)
            description(asHtml: false)
            startDate { year month day }
            endDate { year month day }
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
            trailer { id site thumbnail }
            updatedAt
            coverImage {
              extraLarge
              large
              medium
              color
            }
            bannerImage
            genres
            synonyms
            averageScore
            meanScore
            popularity
            isLocked
            trending
            favourites
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
            isAdult
            nextAiringEpisode {
              id
              airingAt
              timeUntilAiring
              episode
              mediaId
            }
            airingSchedule(perPage: 10, notYetAired: true) {
              nodes {
                id
                airingAt
                timeUntilAiring
                episode
                mediaId
              }
            }
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
            streamingEpisodes {
              title
              thumbnail
              url
              site
            }
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
            siteUrl
            autoCreateForumThread
            isRecommendationBlocked
            isReviewBlocked
            """;

        public const string MediaPage = $$"""
            query ($page: Int, $perPage: Int, $sort: [MediaSort]) {
              Page(page: $page, perPage: $perPage) {
                media(type: ANIME, sort: $sort, isAdult: false) {
                  {{MediaFields}}
                }
              }
            }
            """;

        public const string CurrentSimulcast = $$"""
            query (
              $page: Int,
              $perPage: Int,
              $season: MediaSeason!,
              $seasonYear: Int!,
              $sort: [MediaSort]
            ) {
              Page(page: $page, perPage: $perPage) {
                media(
                  type: ANIME,
                  season: $season,
                  seasonYear: $seasonYear,
                  status: RELEASING,
                  sort: $sort,
                  isAdult: false
                ) {
                  {{MediaFields}}
                }
              }
            }
            """;
    }
}
