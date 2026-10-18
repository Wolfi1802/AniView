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

        // Der Sendeplan enthält jede Folge einzeln. Eine Woche umfasst je laufender Serie etwa eine Folge,
        // die Obergrenze verhindert endloses Blättern bei fehlerhaften Antworten.
        private const int MaxSchedulePages = 30;

        // Zeitfenster, in dem eine Folge ausgestrahlt werden muss, damit die Serie als Simulcast gilt.
        // Eine Woche erfasst alle wöchentlichen Serien; eine Reserve fängt leicht verschobene Termine ab.
        private static readonly TimeSpan AiringWindow = TimeSpan.FromDays(8);

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

            // Die IDs werden einmal vorab bestimmt, damit Seite und Anzahl nicht beide die Abfragen auslösen.
            var ids = await GetSimulcastIdsAsync(season, seasonYear, cancellationToken);

            if (ids.Count == 0)
            {
                return new PageResult<AnimeSummary>([], false, 0, perPage);
            }

            if (options.SortField == AnimeSortField.NextEpisode)
            {
                return await GetPageByNextEpisodeAsync(page, perPage, season, seasonYear, ids, options, cancellationToken);
            }

            var pageTask = GetPageAsync(indexOfPage, perPage, season, seasonYear, ids, options, cancellationToken);
            var totalTask = GetTotalAsync(season, seasonYear, ids, options, cancellationToken);
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
            IReadOnlyList<int> ids,
            AnimeListOptions options,
            CancellationToken cancellationToken)
        {
            var all = await GetAllByNextEpisodeAsync(season, seasonYear, ids, options, cancellationToken);

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
            IReadOnlyList<int> ids,
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
                variables["ids"] = ids;

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
            IReadOnlyList<int> ids,
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
            variables["ids"] = ids;

            var data = await _client.SendAsync<MediaPageData>(
                ApiQueries.CurrentSimulcast(Profile),
                variables,
                cancellationToken);

            var result = AnimeSummaryMapper.MapPage(data, perPage);

            _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }

        /// <summary>
        /// Zählt die Simulcast-Animes exakt, indem die Seiten (nur IDs) durchlaufen werden,
        /// bis eine Seite nicht mehr voll ist.
        /// </summary>
        private async Task<int> GetTotalAsync(
            string season,
            int seasonYear,
            IReadOnlyList<int> ids,
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
                variables["ids"] = ids;

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

        /// <summary>
        /// Ermittelt die IDs aller Animes im Simulcast, unabhängig von Filter und Sortierung.
        /// Das sind alle Animes mit einer Folge im Zeitfenster (laufende Serien, auch aus früheren Seasons,
        /// sowie Serien, die gerade starten) und alle laufenden oder noch nicht gestarteten Animes der Season.
        /// </summary>
        private async Task<IReadOnlyList<int>> GetSimulcastIdsAsync(
            string season,
            int seasonYear,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"anilist:simulcast:ids:{season}:{seasonYear}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<int>? cached) && cached is not null)
            {
                return cached;
            }

            var scheduleTask = GetAiringScheduleIdsAsync(cancellationToken);
            var seasonTask = GetSeasonIdsAsync(season, seasonYear, cancellationToken);
            await Task.WhenAll(scheduleTask, seasonTask);

            IReadOnlyList<int> ids = scheduleTask.Result
                .Union(seasonTask.Result)
                .Order()
                .ToList();

            _cache.Set(cacheKey, ids, CacheDuration);

            return ids;
        }

        private async Task<IReadOnlyCollection<int>> GetAiringScheduleIdsAsync(CancellationToken cancellationToken)
        {
            const int pageSize = ApiPaging.MAX_PAGE_NUMBER;

            var now = DateTimeOffset.UtcNow;
            var from = (int)now.ToUnixTimeSeconds();
            var to = (int)now.Add(AiringWindow).ToUnixTimeSeconds();

            var ids = new HashSet<int>();

            for (var pageIndex = ApiPaging.MIN_PAGE_NUMBER; pageIndex <= MaxSchedulePages; pageIndex++)
            {
                var variables = new Dictionary<string, object?>
                {
                    ["page"] = pageIndex,
                    ["perPage"] = pageSize,
                    ["from"] = from,
                    ["to"] = to
                };

                var data = await _client.SendAsync<AiringSchedulePageData>(
                    ApiQueries.AiringScheduleMediaIds,
                    variables,
                    cancellationToken);

                var entries = data.Page?.AiringSchedules ?? [];

                foreach (var entry in entries)
                {
                    if (entry?.MediaId is int mediaId)
                    {
                        ids.Add(mediaId);
                    }
                }

                if (entries.Count < pageSize)
                {
                    break;
                }
            }

            return ids;
        }

        private async Task<IReadOnlyCollection<int>> GetSeasonIdsAsync(
            string season,
            int seasonYear,
            CancellationToken cancellationToken)
        {
            const int pageSize = ApiPaging.MAX_PAGE_NUMBER;

            var ids = new HashSet<int>();

            for (var pageIndex = ApiPaging.MIN_PAGE_NUMBER; pageIndex <= MaxCountPages; pageIndex++)
            {
                var variables = new Dictionary<string, object?>
                {
                    ["page"] = pageIndex,
                    ["perPage"] = pageSize,
                    ["season"] = season,
                    ["seasonYear"] = seasonYear
                };

                var data = await _client.SendAsync<MediaPageData>(
                    ApiQueries.SeasonMediaIds,
                    variables,
                    cancellationToken);

                var media = data.Page?.Media ?? [];

                foreach (var item in media)
                {
                    if (item is not null)
                    {
                        ids.Add(item.Id);
                    }
                }

                if (media.Count < pageSize)
                {
                    break;
                }
            }

            return ids;
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
