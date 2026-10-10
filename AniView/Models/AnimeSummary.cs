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
        /// Durchschnittsbewertung auf einer Skala von 0 bis 10 oder null, wenn keine Bewertung vorliegt.
        /// </summary>
        public double? ScoreOutOfTen => AverageScore is > 0 ? AverageScore / 10.0 : null;
    }
}
