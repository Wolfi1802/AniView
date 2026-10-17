using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class StreamingSiteService : IStreamingSiteService
    {
        private const string CacheKey = "anilist:streaming-sites";

        // Die Liste ändert sich selten, deshalb lange zwischenspeichern.
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        // Kürzer zwischenspeichern, wenn nur die feste Auswahl verfügbar ist, damit bald erneut versucht wird.
        private static readonly TimeSpan FallbackCacheDuration = TimeSpan.FromMinutes(5);

        // Auswahl, falls AniList die Liste nicht liefert. Die Namen entsprechen den Seitennamen bei AniList.
        private static readonly string[] FallbackSites =
        [
            "Crunchyroll",
            "Netflix",
            "HIDIVE",
            "Hulu",
            "Disney Plus"
        ];

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<StreamingSiteService> _logger;

        public StreamingSiteService(IAniListClient client, IMemoryCache cache, ILogger<StreamingSiteService> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<string>> GetStreamingSitesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            {
                return cached;
            }

            try
            {
                var data = await _client.SendAsync<ExternalLinkSourceData>(
                    ApiQueries.StreamingSites,
                    variables: null,
                    cancellationToken);

                IReadOnlyList<string> sites = (data.ExternalLinkSourceCollection ?? [])
                    .Where(link => link is { IsDisabled: not true } && !string.IsNullOrWhiteSpace(link.Site))
                    .Select(link => link!.Site!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (sites.Count > 0)
                {
                    _cache.Set(CacheKey, sites, CacheDuration);
                    return sites;
                }
            }
            catch (ApiException exception)
            {
                // Der Filter bleibt mit der festen Auswahl nutzbar, ein Fehler hier soll die Seite nicht blockieren.
                _logger.LogWarning(exception, "Die Streaming-Dienste konnten nicht geladen werden, die feste Auswahl wird verwendet.");
            }

            _cache.Set(CacheKey, (IReadOnlyList<string>)FallbackSites, FallbackCacheDuration);
            return FallbackSites;
        }
    }
}
