namespace AniView.Structure.API.Dtos
{
    // Interne Transportmodelle für die AniList-API. Alle Felder sind nullable,
    // weil die API fehlende Werte als null liefert.

    internal sealed record GraphQlRequest(string Query, object? Variables);

    internal sealed class GraphQlResponse<T>
    {
        public T? Data { get; init; }

        public List<GraphQlError>? Errors { get; init; }
    }

    internal sealed class GraphQlError
    {
        public string? Message { get; init; }

        public int? Status { get; init; }
    }

    internal sealed class MediaPageData
    {
        public MediaPageDto? Page { get; init; }
    }

    internal sealed class MediaPageDto
    {
        public List<MediaDto?>? Media { get; init; }
    }

    internal sealed class MediaDto
    {
        public int Id { get; init; }

        public MediaTitleDto? Title { get; init; }

        public MediaCoverDto? CoverImage { get; init; }

        public string? Format { get; init; }

        public int? AverageScore { get; init; }

        public int? Popularity { get; init; }

        public MediaAiringScheduleDto? NextAiringEpisode { get; init; }
    }

    internal sealed class MediaTitleDto
    {
        public string? Romaji { get; init; }

        public string? English { get; init; }
    }

    internal sealed class MediaCoverDto
    {
        public string? Large { get; init; }

        public string? ExtraLarge { get; init; }
    }

    internal sealed class MediaAiringScheduleDto
    {
        /// <summary>
        /// Unix-Zeitstempel der Ausstrahlung in Sekunden.
        /// </summary>
        public int? AiringAt { get; init; }

        /// <summary>
        /// Verbleibende Sekunden bis zur Ausstrahlung.
        /// </summary>
        public int? TimeUntilAiring { get; init; }

        /// <summary>
        /// Nummer der nächsten Episode.
        /// </summary>
        public int? Episode { get; init; }
    }
}
