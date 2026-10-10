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

    /// <summary>
    /// Vollständige Abbildung des AniList-Typs "Media" (ohne nutzerspezifische
    /// Felder, die eine Anmeldung erfordern, und ohne Moderationsfelder).
    /// </summary>
    internal sealed class MediaDto
    {
        public int Id { get; init; }

        public int? IdMal { get; init; }

        public MediaTitleDto? Title { get; init; }

        /// <summary>ANIME oder MANGA.</summary>
        public string? Type { get; init; }

        public string? Format { get; init; }

        public string? Status { get; init; }

        public string? Description { get; init; }

        public FuzzyDateDto? StartDate { get; init; }

        public FuzzyDateDto? EndDate { get; init; }

        public string? Season { get; init; }

        public int? SeasonYear { get; init; }

        public int? SeasonInt { get; init; }

        public int? Episodes { get; init; }

        /// <summary>Dauer einer Episode in Minuten.</summary>
        public int? Duration { get; init; }

        public int? Chapters { get; init; }

        public int? Volumes { get; init; }

        public string? CountryOfOrigin { get; init; }

        public bool? IsLicensed { get; init; }

        public string? Source { get; init; }

        public string? Hashtag { get; init; }

        public MediaTrailerDto? Trailer { get; init; }

        /// <summary>Unix-Zeitstempel der letzten Aktualisierung in Sekunden.</summary>
        public int? UpdatedAt { get; init; }

        public MediaCoverDto? CoverImage { get; init; }

        public string? BannerImage { get; init; }

        public List<string?>? Genres { get; init; }

        public List<string?>? Synonyms { get; init; }

        public int? AverageScore { get; init; }

        public int? MeanScore { get; init; }

        public int? Popularity { get; init; }

        public bool? IsLocked { get; init; }

        public int? Trending { get; init; }

        public int? Favourites { get; init; }

        public List<MediaTagDto?>? Tags { get; init; }

        public EdgeListDto<MediaRelationEdgeDto>? Relations { get; init; }

        public EdgeListDto<CharacterEdgeDto>? Characters { get; init; }

        public EdgeListDto<StaffEdgeDto>? Staff { get; init; }

        public EdgeListDto<StudioEdgeDto>? Studios { get; init; }

        public bool? IsAdult { get; init; }

        public MediaAiringScheduleDto? NextAiringEpisode { get; init; }

        public NodeListDto<MediaAiringScheduleDto>? AiringSchedule { get; init; }

        public NodeListDto<MediaTrendDto>? Trends { get; init; }

        public List<MediaExternalLinkDto?>? ExternalLinks { get; init; }

        public List<MediaStreamingEpisodeDto?>? StreamingEpisodes { get; init; }

        public List<MediaRankDto?>? Rankings { get; init; }

        public NodeListDto<RecommendationDto>? Recommendations { get; init; }

        public NodeListDto<ReviewDto>? Reviews { get; init; }

        public string? SiteUrl { get; init; }

        public bool? AutoCreateForumThread { get; init; }

        public bool? IsRecommendationBlocked { get; init; }

        public bool? IsReviewBlocked { get; init; }
    }

    /// <summary>
    /// Allgemeiner Container für AniList-Verbindungen, die ihre Daten als "edges" liefern.
    /// </summary>
    internal sealed class EdgeListDto<T>
    {
        public List<T?>? Edges { get; init; }
    }

    /// <summary>
    /// Allgemeiner Container für AniList-Verbindungen, die ihre Daten als "nodes" liefern.
    /// </summary>
    internal sealed class NodeListDto<T>
    {
        public List<T?>? Nodes { get; init; }
    }

    internal sealed class FuzzyDateDto
    {
        public int? Year { get; init; }

        public int? Month { get; init; }

        public int? Day { get; init; }
    }

    internal sealed class MediaTitleDto
    {
        public string? Romaji { get; init; }

        public string? English { get; init; }

        public string? Native { get; init; }

        public string? UserPreferred { get; init; }
    }

    internal sealed class MediaCoverDto
    {
        public string? Large { get; init; }

        public string? ExtraLarge { get; init; }

        public string? Medium { get; init; }

        /// <summary>Durchschnittsfarbe des Covers als Hex-Wert.</summary>
        public string? Color { get; init; }
    }

    internal sealed class MediaTrailerDto
    {
        public string? Id { get; init; }

        public string? Site { get; init; }

        public string? Thumbnail { get; init; }
    }

    internal sealed class MediaTagDto
    {
        public int? Id { get; init; }

        public string? Name { get; init; }

        public string? Description { get; init; }

        public string? Category { get; init; }

        public int? Rank { get; init; }

        public bool? IsGeneralSpoiler { get; init; }

        public bool? IsMediaSpoiler { get; init; }

        public bool? IsAdult { get; init; }
    }

    internal sealed class MediaAiringScheduleDto
    {
        public int? Id { get; init; }

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

        public int? MediaId { get; init; }
    }

    internal sealed class MediaTrendDto
    {
        public int? MediaId { get; init; }

        /// <summary>Unix-Zeitstempel des Messtags in Sekunden.</summary>
        public int? Date { get; init; }

        public int? Trending { get; init; }

        public int? AverageScore { get; init; }

        public int? Popularity { get; init; }

        public bool? InProgress { get; init; }

        public bool? Releasing { get; init; }

        public int? Episode { get; init; }
    }

    internal sealed class MediaExternalLinkDto
    {
        public int? Id { get; init; }

        public string? Url { get; init; }

        public string? Site { get; init; }

        public int? SiteId { get; init; }

        /// <summary>INFO, STREAMING oder SOCIAL.</summary>
        public string? Type { get; init; }

        public string? Language { get; init; }

        public string? Color { get; init; }

        public string? Icon { get; init; }

        public string? Notes { get; init; }

        public bool? IsDisabled { get; init; }
    }

    internal sealed class MediaStreamingEpisodeDto
    {
        public string? Title { get; init; }

        public string? Thumbnail { get; init; }

        public string? Url { get; init; }

        public string? Site { get; init; }
    }

    internal sealed class MediaRankDto
    {
        public int? Id { get; init; }

        public int? Rank { get; init; }

        /// <summary>RATED oder POPULAR.</summary>
        public string? Type { get; init; }

        public string? Format { get; init; }

        public int? Year { get; init; }

        public string? Season { get; init; }

        public bool? AllTime { get; init; }

        public string? Context { get; init; }
    }

    internal sealed class ImageDto
    {
        public string? Large { get; init; }

        public string? Medium { get; init; }
    }

    internal sealed class PersonNameDto
    {
        public string? Full { get; init; }

        public string? Native { get; init; }
    }

    internal sealed class MediaRelationEdgeDto
    {
        public int? Id { get; init; }

        public string? RelationType { get; init; }

        public RelatedMediaDto? Node { get; init; }
    }

    internal sealed class RelatedMediaDto
    {
        public int Id { get; init; }

        public MediaTitleDto? Title { get; init; }

        public string? Type { get; init; }

        public string? Format { get; init; }

        public string? Status { get; init; }

        public MediaCoverDto? CoverImage { get; init; }
    }

    internal sealed class CharacterEdgeDto
    {
        public int? Id { get; init; }

        /// <summary>MAIN, SUPPORTING oder BACKGROUND.</summary>
        public string? Role { get; init; }

        public CharacterDto? Node { get; init; }
    }

    internal sealed class CharacterDto
    {
        public int Id { get; init; }

        public PersonNameDto? Name { get; init; }

        public ImageDto? Image { get; init; }
    }

    internal sealed class StaffEdgeDto
    {
        public int? Id { get; init; }

        public string? Role { get; init; }

        public StaffDto? Node { get; init; }
    }

    internal sealed class StaffDto
    {
        public int Id { get; init; }

        public PersonNameDto? Name { get; init; }

        public ImageDto? Image { get; init; }
    }

    internal sealed class StudioEdgeDto
    {
        public int? Id { get; init; }

        public bool? IsMain { get; init; }

        public StudioDto? Node { get; init; }
    }

    internal sealed class StudioDto
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public bool? IsAnimationStudio { get; init; }

        public string? SiteUrl { get; init; }
    }

    internal sealed class RecommendationDto
    {
        public int? Id { get; init; }

        public int? Rating { get; init; }

        public RelatedMediaDto? MediaRecommendation { get; init; }
    }

    internal sealed class ReviewDto
    {
        public int? Id { get; init; }

        public string? Summary { get; init; }

        public int? Rating { get; init; }

        public int? RatingAmount { get; init; }

        public int? Score { get; init; }

        /// <summary>Unix-Zeitstempel der Erstellung in Sekunden.</summary>
        public int? CreatedAt { get; init; }

        public ReviewUserDto? User { get; init; }
    }

    internal sealed class ReviewUserDto
    {
        public int? Id { get; init; }

        public string? Name { get; init; }
    }
}
