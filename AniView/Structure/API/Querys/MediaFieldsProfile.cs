namespace AniView.Structure.API.Querys
{
    /// <summary>
    /// Legt fest, welche Objekte der zweiten Ebene (zum Beispiel Reviews oder
    /// Empfehlungen) in der Feldauswahl einer Media-Abfrage enthalten sind.
    /// Einfache Felder der ersten Ebene (id, format, averageScore, ...) sind immer enthalten.
    /// </summary>
    internal sealed class MediaFieldsProfile
    {
        public bool Title { get; init; }

        public bool StartDate { get; init; }

        public bool EndDate { get; init; }

        public bool Trailer { get; init; }

        public bool CoverImage { get; init; }

        public bool Tags { get; init; }

        public bool Relations { get; init; }

        public bool Characters { get; init; }

        public bool Staff { get; init; }

        public bool Studios { get; init; }

        public bool NextAiringEpisode { get; init; }

        public bool AiringSchedule { get; init; }

        public bool Trends { get; init; }

        public bool ExternalLinks { get; init; }

        public bool StreamingEpisodes { get; init; }

        public bool Rankings { get; init; }

        public bool Recommendations { get; init; }

        public bool Reviews { get; init; }


        /// <summary>
        /// Nur die Objekte, die Karten und Ranglisten benötigen.
        /// </summary>
        public static MediaFieldsProfile Summary { get; } = new()
        {
            Title = true,
            StartDate = true,
            EndDate = true,
            CoverImage = true,
            NextAiringEpisode = true
        };

        /// <summary>
        /// Alle Objekte der zweiten Ebene.
        /// </summary>
        public static MediaFieldsProfile Full { get; } = new()
        {
            Title = true,
            StartDate = true,
            EndDate = true,
            Trailer = true,
            CoverImage = true,
            Tags = true,
            Relations = true,
            Characters = true,
            Staff = true,
            Studios = true,
            NextAiringEpisode = true,
            AiringSchedule = true,
            Trends = true,
            ExternalLinks = true,
            StreamingEpisodes = true,
            Rankings = true,
            Recommendations = true,
            Reviews = true
        };

        /// <summary>
        /// Alle Objekte der zweiten Ebene, die die Detailansicht anzeigt (ohne verwandte Titel und Empfehlungen).
        /// </summary>
        public static MediaFieldsProfile Detail { get; } = new()
        {
            Title = true,
            StartDate = true,
            EndDate = true,
            Trailer = true,
            CoverImage = true,
            Tags = true,
            Characters = true,
            Staff = true,
            Studios = true,
            NextAiringEpisode = true,
            AiringSchedule = true,
            Trends = true,
            ExternalLinks = true,
            Rankings = true,
            Reviews = true
        };

        /// <summary>
        /// Eindeutiger Schlüssel der aktiven Objekte, damit unterschiedliche Profile
        /// im Cache nicht dieselben Einträge teilen.
        /// </summary>
        public string CacheKey => string.Concat(Flags.Select(enabled => enabled ? '1' : '0'));

        private bool[] Flags =>
        [
            Title, StartDate, EndDate, Trailer, CoverImage, Tags,
            Relations, Characters, Staff, Studios, NextAiringEpisode,
            AiringSchedule, Trends, ExternalLinks, StreamingEpisodes,
            Rankings, Recommendations, Reviews
        ];
    }
}
