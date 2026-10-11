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

            var result = AnimeSummaryMapper.MapAll(data);

            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        public async Task<PageResult<AnimeSummary>> GetPopularPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            int perPage = ApiPaging.GetPageCount(pageSize);
            var variables = new { page = ApiPaging.GetPageIndex(page), perPage, sort = new[] { "POPULARITY_DESC" } };
            var data = await _client.SendAsync<MediaPageData>(ApiQueries.MediaPage(Profile), variables, cancellationToken);

            var items = AnimeSummaryMapper.MapAll(data);

            // Die Abfrage liefert keine Seiteninfo. Eine volle Seite gilt als "es gibt eventuell mehr".
            return new PageResult<AnimeSummary>(items, items.Count >= perPage);
        }
    }
}
