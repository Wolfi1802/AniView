namespace AniView.Structure.API.Querys
{
    /// <summary>
    /// GraphQL-Abfragen für AniList. Wird nur vom Service verwendet, nicht von Komponenten.
    /// </summary>
    internal static class ApiQueries
    {
        public const string MediaPage = """
            query ($page: Int, $perPage: Int, $sort: [MediaSort]) {
              Page(page: $page, perPage: $perPage) {
                media(type: ANIME, sort: $sort, isAdult: false) {
                  id
                  title {
                    romaji
                    english
                  }
                  coverImage {
                    large
                    extraLarge
                  }
                  format
                  averageScore
                  popularity
                  nextAiringEpisode {
                    airingAt
                    timeUntilAiring
                    episode
                  }
                }
              }
            }
            """;

        public const string CurrentSimulcast = """
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
                  id
                  title {
                    romaji
                    english
                  }
                  coverImage {
                    large
                    extraLarge
                  }
                  format
                  averageScore
                  popularity
                  nextAiringEpisode {
                    airingAt
                    timeUntilAiring
                    episode
                  }
                }
              }
            }
            """;
    }
}