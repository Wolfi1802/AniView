using AniView.Models;
using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class TagService : ITagService
    {
        private const string CacheKey = "anilist:tags";

        // Die Liste ändert sich selten, deshalb lange zwischenspeichern.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        // Kürzer zwischenspeichern, wenn nur die feste Auswahl verfügbar ist, damit bald erneut versucht wird.
        private static readonly TimeSpan FallbackCacheDuration = TimeSpan.FromMinutes(5);

        // Auswahl, falls AniList die Liste nicht liefert: die Zielgruppen. Die Namen entsprechen den Tags bei AniList.
        private static readonly AnimeTagInfo[] FallbackTags =
        [
            new("Josei", "Demographic"),
            new("Kids", "Demographic"),
            new("Seinen", "Demographic"),
            new("Shoujo", "Demographic"),
            new("Shounen", "Demographic")
        ];

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TagService> _logger;

        public TagService(IAniListClient client, IMemoryCache cache, ILogger<TagService> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AnimeTagInfo>> GetTagsAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<AnimeTagInfo>? cached) && cached is not null)
            {
                return cached;
            }

            try
            {
                var data = await _client.SendAsync<MediaTagCollectionData>(
                    ApiQueries.Tags,
                    variables: null,
                    cancellationToken);

                // Alle Listen der Seite blenden Inhalte für Erwachsene aus, deshalb werden solche Tags nicht angeboten.
                IReadOnlyList<AnimeTagInfo> tags = (data.MediaTagCollection ?? [])
                    .Where(tag => tag is { IsAdult: not true } && !string.IsNullOrWhiteSpace(tag.Name))
                    .Select(tag => new AnimeTagInfo(tag!.Name!.Trim(), tag.Category?.Trim() ?? string.Empty))
                    .DistinctBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(tag => tag.Category, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (tags.Count > 0)
                {
                    _cache.Set(CacheKey, tags, CacheDuration);
                    return tags;
                }
            }
            catch (ApiException exception)
            {
                // Der Filter bleibt mit der festen Auswahl nutzbar, ein Fehler hier soll die Seite nicht blockieren.
                _logger.LogWarning(exception, "Die Tags konnten nicht geladen werden, die feste Auswahl wird verwendet.");
            }

            _cache.Set(CacheKey, (IReadOnlyList<AnimeTagInfo>)FallbackTags, FallbackCacheDuration);
            return FallbackTags;
        }
    }
}
