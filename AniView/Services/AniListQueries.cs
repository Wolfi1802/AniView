namespace AniView.Services
{
    /// <summary>
    /// GraphQL-Abfragen für AniList. Wird nur vom Service verwendet, nicht von Komponenten.
    /// </summary>
    internal static class AniListQueries
    {
        public const string MediaPage = """
            query ($page: Int, $perPage: Int, $sort: [MediaSort]) {
              Page(page: $page, perPage: $perPage) {
                media(type: ANIME, sort: $sort, isAdult: false) {
                  id
                  title { romaji english }
                  coverImage { large extraLarge }
                  format
                  averageScore
                  popularity
                }
              }
            }
            """;
    }
}
