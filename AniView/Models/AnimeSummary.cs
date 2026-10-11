namespace AniView.Models
{
    /// <summary>
    /// Kompakte Darstellung eines Animes für Karten und Ranglisten.
    /// </summary>
    public sealed record AnimeSummary(
        int Id,
        string Title,
        string? CoverUrl,
        string? Format,
        int? AverageScore,
        int? Popularity)
    {
        /// <summary>
        /// Durchschnittsbewertung auf einer Skala von 0 bis 10.
        /// </summary>
        public double? ScoreOutOfTen =>
            AverageScore is > 0 ? AverageScore / 10.0 : null;

        /// <summary>
        /// Gibt an, ob der Anime vollständig erschienen ist (Status FINISHED bei AniList).
        /// </summary>
        public bool IsFinished { get; init; }

        /// <summary>
        /// Anzahl der Episoden, sofern bekannt (bei laufenden Serien oft noch nicht festgelegt).
        /// </summary>
        public int? Episodes { get; init; }

        /// <summary>
        /// Nummer der nächsten Episode.
        /// </summary>
        public int? NextEpisodeNumber { get; init; }

        /// <summary>
        /// Zeitpunkt der nächsten Episode in UTC.
        /// </summary>
        public DateTimeOffset? NextEpisodeAiringAt { get; init; }

        /// <summary>
        /// Verbleibende Zeit bis zur nächsten Episode.
        /// </summary>
        public TimeSpan? TimeUntilNextEpisode { get; init; }
    }
}
