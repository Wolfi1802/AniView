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
            var page = await GetCurrentSimulcastPageAsync(pageIndex, itemsPerPage, AnimeListOptions.Default, cancellationToken);
            return page.Items;
        }

        public async Task<PageResult<AnimeSummary>> GetCurrentSimulcastPageAsync(
            int page,
            int pageSize,
            AnimeListOptions options,
            CancellationToken cancellationToken = default)
        {
            int indexOfPage = ApiPaging.GetPageIndex(page);
            int perPage = ApiPaging.GetPageCount(pageSize);

            var (season, seasonYear) = GetCurrentSeason();

            if (options.SortField == AnimeSortField.NextEpisode)
            {
                return await GetPageByNextEpisodeAsync(page, perPage, season, seasonYear, options, cancellationToken);
            }

            var pageTask = GetPageAsync(indexOfPage, perPage, season, seasonYear, options, cancellationToken);
            var totalTask = GetTotalAsync(season, seasonYear, options, cancellationToken);
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

        /// <summary>
        /// AniList kann nicht nach der nächsten Episode sortieren. Der Simulcast ist begrenzt, deshalb wird er mit dem
        /// Filter vollständig geladen, nach dem Zeitpunkt der nächsten Episode sortiert und seitenweise ausgeliefert.
        /// </summary>
        private async Task<PageResult<AnimeSummary>> GetPageByNextEpisodeAsync(
            int page,
            int perPage,
            string season,
            int seasonYear,
            AnimeListOptions options,
            CancellationToken cancellationToken)
        {
            var all = await GetAllByNextEpisodeAsync(season, seasonYear, options, cancellationToken);

            var skip = (long)(ApiPaging.GetPageIndex(page) - 1) * perPage;
            IReadOnlyList<AnimeSummary> items = all
                .Skip((int)Math.Min(skip, all.Count))
                .Take(perPage)
                .ToList();

            return new PageResult<AnimeSummary>(items, skip + items.Count < all.Count, all.Count, perPage);
        }

        private async Task<IReadOnlyList<AnimeSummary>> GetAllByNextEpisodeAsync(
            string season,
            int seasonYear,
            AnimeListOptions options,
            CancellationToken cancellationToken)
        {
            // Die Reihenfolge hängt von der Richtung ab, die geladene Liste nur vom Filter.
            var cacheKey =
                $"anilist:simulcast:next-episode:{season}:{seasonYear}:{Profile.CacheKey}:{options.FilterKey}:{(options.Descending ? "desc" : "asc")}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AnimeSummary>? cached) && cached is not null)
            {
                return cached;
            }

            const int pageSize = ApiPaging.MAX_PAGE_NUMBER;
            var animes = new List<AnimeSummary>();

            for (var pageIndex = ApiPaging.MIN_PAGE_NUMBER; pageIndex <= MaxCountPages; pageIndex++)
            {
                var variables = options.ToVariables(pageIndex, pageSize);
                variables["season"] = season;
                variables["seasonYear"] = seasonYear;

                var data = await _client.SendAsync<MediaPageData>(
                    ApiQueries.CurrentSimulcast(Profile),
                    variables,
                    cancellationToken);

                var items = AnimeSummaryMapper.MapAll(data);
                animes.AddRange(items);

                if (items.Count < pageSize)
                {
                    break;
                }
            }

            // Ohne bekannten Termin steht ein Anime immer am Ende, unabhängig von der Richtung.
            var withDate = animes.Where(anime => anime.NextEpisodeAiringAt is not null);
            var ordered = (options.Descending
                    ? withDate.OrderByDescending(anime => anime.NextEpisodeAiringAt)
                    : withDate.OrderBy(anime => anime.NextEpisodeAiringAt))
                .Concat(animes.Where(anime => anime.NextEpisodeAiringAt is null))
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<AnimeSummary>)ordered, CacheDuration);

            return ordered;
        }

        private async Task<PageResult<AnimeSummary>> GetPageAsync(
            int indexOfPage,
            int perPage,
            string season,
            int seasonYear,
            AnimeListOptions options,
            CancellationToken cancellationToken)
        {
            // Season, Jahr, Pagination, Profil sowie Sortierung und Filter müssen Teil des Cache-Keys sein.
            var cacheKey =
                $"anilist:simulcast:{season}:{seasonYear}:{indexOfPage}:{perPage}:{Profile.CacheKey}:{options.CacheKey}";

            if (_cache.TryGetValue(
                cacheKey,
                out PageResult<AnimeSummary>? cached)
                && cached is not null)
            {
                return cached;
            }

            var variables = options.ToVariables(indexOfPage, perPage);
            variables["season"] = season;
            variables["seasonYear"] = seasonYear;

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
        private async Task<int> GetTotalAsync(
            string season,
            int seasonYear,
            AnimeListOptions options,
            CancellationToken cancellationToken)
        {
            // Die Anzahl hängt nur vom Filter ab, nicht von der Sortierung.
            var cacheKey = $"anilist:simulcast:total:{season}:{seasonYear}:{options.FilterKey}";

            if (_cache.TryGetValue(cacheKey, out int cached))
            {
                return cached;
            }

            const int countPageSize = ApiPaging.MAX_PAGE_NUMBER;
            var total = 0;

            for (var pageIndex = ApiPaging.MIN_PAGE_NUMBER; pageIndex <= MaxCountPages; pageIndex++)
            {
                var variables = options.ToFilterVariables();
                variables["page"] = pageIndex;
                variables["perPage"] = countPageSize;
                variables["season"] = season;
                variables["seasonYear"] = seasonYear;

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
