using AniView.Models;
using AniView.Structure.API.Dtos;
using AniView.Structure.API.Mapping;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class PopularAnimeService : IPopularAnimeService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        // Bestimmt, welche Objekte der zweiten Ebene diese Abfrage lädt.
        private static readonly MediaFieldsProfile Profile = MediaFieldsProfile.Summary;

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;

        public PopularAnimeService(IAniListClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
        {
            int perPage = ApiPaging.GetPageCount(count);
            var cacheKey = $"anilist:popular:{perPage}:{Profile.CacheKey}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AnimeSummary>? cached) && cached is not null)
            {
                return cached;
            }

            var variables = new { page = 1, perPage, sort = new[] { "POPULARITY_DESC" } };
            var data = await _client.SendAsync<MediaPageData>(ApiQueries.MediaPage(Profile), variables, cancellationToken);

            // AniList sortiert bereits nach Beliebtheit. Die Sortierung hier stellt sicher, dass die Reihenfolge
            // und der Rang immer zur angezeigten Beliebtheit passen (bei gleichem Wert bleibt die Reihenfolge der API).
            IReadOnlyList<AnimeSummary> result = AnimeSummaryMapper.MapAll(data)
                .OrderByDescending(anime => anime.Popularity ?? 0)
                .ToList();

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        public async Task<PageResult<AnimeSummary>> GetPopularPageAsync(
            int page,
            int pageSize,
            AnimeListOptions options,
            CancellationToken cancellationToken = default)
        {
            int perPage = ApiPaging.GetPageCount(pageSize);
            var variables = options.ToVariables(ApiPaging.GetPageIndex(page), perPage);
            var data = await _client.SendAsync<MediaPageData>(ApiQueries.MediaPage(Profile), variables, cancellationToken);

            return AnimeSummaryMapper.MapPage(data, perPage);
        }
    }
}
