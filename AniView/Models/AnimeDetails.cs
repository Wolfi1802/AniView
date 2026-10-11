namespace AniView.Models
{
    /// <summary>
    /// Alle Informationen zu einem Anime, die AniList liefert. Wird erst beim Öffnen des Detail-Dialogs nachgeladen.
    /// Werte wie Status, Quelle oder Rollen sind bereits ins Deutsche übersetzt.
    /// </summary>
    public sealed record AnimeDetails(int Id, string Title)
    {
        // Titel und Bilder
        public string? RomajiTitle { get; init; }

        public string? EnglishTitle { get; init; }

        public string? NativeTitle { get; init; }

        public IReadOnlyList<string> Synonyms { get; init; } = [];

        public string? CoverUrl { get; init; }

        public string? BannerUrl { get; init; }

        public string? Description { get; init; }

        // Allgemeine Angaben
        public string? Format { get; init; }

        public string? Status { get; init; }

        public string? Season { get; init; }

        public string? StartDateText { get; init; }

        public string? EndDateText { get; init; }

        public int? Episodes { get; init; }

        /// <summary>Dauer einer Episode in Minuten.</summary>
        public int? DurationMinutes { get; init; }

        public string? Source { get; init; }

        public string? CountryOfOrigin { get; init; }

        public bool? IsLicensed { get; init; }

        public string? Hashtag { get; init; }

        public IReadOnlyList<string> Genres { get; init; } = [];

        // Zahlen
        public int? AverageScore { get; init; }

        public int? MeanScore { get; init; }

        public int? Popularity { get; init; }

        public int? Favourites { get; init; }

        public int? Trending { get; init; }

        public int? MalId { get; init; }

        public string? SiteUrl { get; init; }

        public DateTimeOffset? UpdatedAt { get; init; }

        public AnimeTrailer? Trailer { get; init; }

        // Listen
        public IReadOnlyList<AnimeTag> Tags { get; init; } = [];

        public IReadOnlyList<AnimeStudio> Studios { get; init; } = [];

        public IReadOnlyList<AnimePerson> Characters { get; init; } = [];

        public IReadOnlyList<AnimePerson> Staff { get; init; } = [];

        public IReadOnlyList<AnimeExternalLink> ExternalLinks { get; init; } = [];

        public IReadOnlyList<AnimeRanking> Rankings { get; init; } = [];

        public IReadOnlyList<AnimeReview> Reviews { get; init; } = [];

        public IReadOnlyList<AnimeTrendPoint> Trends { get; init; } = [];

        public AnimeAiring? NextEpisode { get; init; }

        public IReadOnlyList<AnimeAiring> UpcomingEpisodes { get; init; } = [];

        public double? ScoreOutOfTen => AverageScore is > 0 ? AverageScore / 10.0 : null;
    }

    public sealed record AnimeTrailer(string Id, string Site, string? ThumbnailUrl)
    {
        /// <summary>Link zum Video, sofern die Plattform bekannt ist.</summary>
        public string? Url => Site.ToLowerInvariant() switch
        {
            "youtube" => $"https://www.youtube.com/watch?v={Uri.EscapeDataString(Id)}",
            "dailymotion" => $"https://www.dailymotion.com/video/{Uri.EscapeDataString(Id)}",
            _ => null
        };
    }

    public sealed record AnimeTag(string Name, string? Description, string? Category, int? Rank, bool IsSpoiler);

    public sealed record AnimeStudio(string Name, bool IsMain, bool IsAnimationStudio, string? Url);

    /// <summary>Charakter oder Mitarbeiter mit Rolle.</summary>
    public sealed record AnimePerson(string Name, string? NativeName, string? Role, string? ImageUrl);

    public sealed record AnimeExternalLink(string Site, string Url, string? Type, string? Language, string? Color, string? Notes);

    public sealed record AnimeRanking(int Rank, string? Type, string? Context, int? Year, string? Season, bool AllTime);

    public sealed record AnimeReview(string? Summary, int? Score, int? Rating, int? RatingAmount, string? UserName, DateTimeOffset? CreatedAt);

    public sealed record AnimeTrendPoint(DateTimeOffset? Date, int? Trending, int? Popularity, int? AverageScore, int? InProgress, int? Episode);

    public sealed record AnimeAiring(int Episode, DateTimeOffset? AiringAt);
}
