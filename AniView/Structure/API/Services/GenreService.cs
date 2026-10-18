using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class GenreService : IGenreService
    {
        private const string CacheKey = "anilist:genres";

        // Die Liste ändert sich selten, deshalb lange zwischenspeichern.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        // Kürzer zwischenspeichern, wenn nur die feste Auswahl verfügbar ist, damit bald erneut versucht wird.
        private static readonly TimeSpan FallbackCacheDuration = TimeSpan.FromMinutes(5);

        // Alle Listen der Seite blenden Inhalte für Erwachsene aus, deshalb wird dieses Genre nicht angeboten.
        private static readonly string[] ExcludedGenres = ["Hentai"];

        // Auswahl, falls AniList die Liste nicht liefert. Die Namen entsprechen den Genres bei AniList.
        private static readonly string[] FallbackGenres =
        [
            "Action",
            "Adventure",
            "Comedy",
            "Drama",
            "Ecchi",
            "Fantasy",
            "Horror",
            "Mahou Shoujo",
            "Mecha",
            "Music",
            "Mystery",
            "Psychological",
            "Romance",
            "Sci-Fi",
            "Slice of Life",
            "Sports",
            "Supernatural",
            "Thriller"
        ];

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GenreService> _logger;

        public GenreService(IAniListClient client, IMemoryCache cache, ILogger<GenreService> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            {
                return cached;
            }

            try
            {
                var data = await _client.SendAsync<GenreCollectionData>(
                    ApiQueries.Genres,
                    variables: null,
                    cancellationToken);

                IReadOnlyList<string> genres = (data.GenreCollection ?? [])
                    .Where(genre => !string.IsNullOrWhiteSpace(genre))
                    .Select(genre => genre!.Trim())
                    .Where(genre => !ExcludedGenres.Contains(genre, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (genres.Count > 0)
                {
                    _cache.Set(CacheKey, genres, CacheDuration);
                    return genres;
                }
            }
            catch (ApiException exception)
            {
                // Der Filter bleibt mit der festen Auswahl nutzbar, ein Fehler hier soll die Seite nicht blockieren.
                _logger.LogWarning(exception, "Die Genres konnten nicht geladen werden, die feste Auswahl wird verwendet.");
            }

            _cache.Set(CacheKey, (IReadOnlyList<string>)FallbackGenres, FallbackCacheDuration);
            return FallbackGenres;
        }
    }
}
