using AniView.Models;
using AniView.Structure.API.Dtos;
using AniView.Structure.API.Mapping;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class CurrentSimulcastService : ICurrentSimulcastService
    {
        private const int DefaultPageIndex = ApiPaging.MIN_PAGE_NUMBER;
        private const int DefaultItemsPerPage = ApiPaging.MAX_PAGE_NUMBER;

        // Obergrenze beim Zählen, damit eine fehlerhafte Antwort nie endlos weiterblättert.
        private const int MaxCountPages = 20;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        // Bestimmt, welche Objekte der zweiten Ebene diese Abfrage lädt.
        private static readonly MediaFieldsProfile Profile = MediaFieldsProfile.Summary;

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;

        public CurrentSimulcastService(IAniListClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task<IReadOnlyList<AnimeSummary>> GetCurrentSimulcast(
            int pageIndex = DefaultPageIndex,
            int itemsPerPage = DefaultItemsPerPage,
            CancellationToken cancellationToken = default)
        {
            var page = await GetCurrentSimulcastPageAsync(pageIndex, itemsPerPage, cancellationToken);
            return page.Items;
        }

        public async Task<PageResult<AnimeSummary>> GetCurrentSimulcastPageAsync(
            int page = DefaultPageIndex,
            int pageSize = DefaultItemsPerPage,
            CancellationToken cancellationToken = default)
        {
            int indexOfPage = ApiPaging.GetPageIndex(page);
            int perPage = ApiPaging.GetPageCount(pageSize);

            var (season, seasonYear) = GetCurrentSeason();

            var pageTask = GetPageAsync(indexOfPage, perPage, season, seasonYear, cancellationToken);
            var totalTask = GetTotalAsync(season, seasonYear, cancellationToken);
            await Task.WhenAll(pageTask, totalTask);

            var total = totalTask.Result;

            // "pageInfo.total" und "hasNextPage" von AniList sind Schätzungen. Maßgeblich ist die selbst gezählte Gesamtanzahl.
            return pageTask.Result with
            {
                TotalItems = total,
                ItemsPerPage = perPage,
                HasNextPage = (long)indexOfPage * perPage < total
            };
        }

        private async Task<PageResult<AnimeSummary>> GetPageAsync(
            int indexOfPage,
            int perPage,
            string season,
            int seasonYear,
            CancellationToken cancellationToken)
        {
            // Season, Jahr, Pagination und Profil müssen Teil des Cache-Keys sein.
            var cacheKey =
                $"anilist:simulcast:{season}:{seasonYear}:{indexOfPage}:{perPage}:{Profile.CacheKey}";

            if (_cache.TryGetValue(
                cacheKey,
                out PageResult<AnimeSummary>? cached)
                && cached is not null)
            {
                return cached;
            }

            var variables = new
            {
                page = indexOfPage,
                perPage,
                season,
                seasonYear,
                sort = new[] { "POPULARITY_DESC" }
            };

            var data = await _client.SendAsync<MediaPageData>(
                ApiQueries.CurrentSimulcast(Profile),
                variables,
                cancellationToken);

            var result = AnimeSummaryMapper.MapPage(data, perPage);

            _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }

        /// <summary>
        /// Zählt die laufenden Animes der Season exakt, indem die Seiten (nur IDs) durchlaufen werden,
        /// bis eine Seite nicht mehr voll ist.
        /// </summary>
        private async Task<int> GetTotalAsync(string season, int seasonYear, CancellationToken cancellationToken)
        {
            var cacheKey = $"anilist:simulcast:total:{season}:{seasonYear}";

            if (_cache.TryGetValue(cacheKey, out int cached))
            {
                return cached;
            }

            const int countPageSize = ApiPaging.MAX_PAGE_NUMBER;
            var total = 0;

            for (var pageIndex = ApiPaging.MIN_PAGE_NUMBER; pageIndex <= MaxCountPages; pageIndex++)
            {
                var variables = new { page = pageIndex, perPage = countPageSize, season, seasonYear };

                var data = await _client.SendAsync<MediaPageData>(
                    ApiQueries.CurrentSimulcastIds,
                    variables,
                    cancellationToken);

                var count = data.Page?.Media?.Count(media => media is not null) ?? 0;
                total += count;

                if (count < countPageSize)
                {
                    break;
                }
            }

            _cache.Set(cacheKey, total, CacheDuration);

            return total;
        }

        private static (string Season, int Year) GetCurrentSeason()
        {
            var now = DateTime.Now;

            string season = now.Month switch
            {
                >= 1 and <= 3 => "WINTER",
                >= 4 and <= 6 => "SPRING",
                >= 7 and <= 9 => "SUMMER",
                _ => "FALL"
            };

            return (season, now.Year);
        }
    }
}
