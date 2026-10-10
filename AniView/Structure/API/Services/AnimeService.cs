using AniView.Models;
using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace AniView.Structure.API.Services
{
    public sealed class AnimeService : IAnimeListService
    {
        private const int MIN_PAGE_NUMBER = 1;
        private const int MAX_PAGE_NUMBER = 50;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private static readonly TimeSpan PopularCacheDuration = TimeSpan.FromMinutes(10);

        private static readonly Dictionary<string, string> FormatLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["TV"] = "TV",
            ["TV_SHORT"] = "TV-Kurzserie",
            ["MOVIE"] = "Film",
            ["SPECIAL"] = "Special",
            ["OVA"] = "OVA",
            ["ONA"] = "ONA",
            ["MUSIC"] = "Musik"
        };

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnimeService> _logger;

        public AnimeService(HttpClient http, IMemoryCache cache, ILogger<AnimeService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        // TODO requests in eigenen pro Kategorie kapseln
        public async Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
        {
            int perPage = GetPageCount(count);
            var cacheKey = $"anilist:popular:{perPage}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AnimeSummary>? cached) && cached is not null)
            {
                return cached;
            }

            var variables = new { page = 1, perPage, sort = new[] { "POPULARITY_DESC" } };
            var data = await SendAsync<MediaPageData>(ApiQueries.MediaPage, variables, cancellationToken);

            IReadOnlyList<AnimeSummary> result = (data.Page?.Media ?? [])
                .Where(media => media is not null)
                .Select(media => MapToSummary(media!))
                .ToList();

            _cache.Set(cacheKey, result, PopularCacheDuration);
            return result;
        }

        public async Task<IReadOnlyList<AnimeSummary>> GetCurrentSimulcast(
            int pageIndex = MIN_PAGE_NUMBER,
            int itemsPerPage = MAX_PAGE_NUMBER,
            CancellationToken cancellationToken = default)
        {
            int indexOfPage = GetPageIndex(pageIndex);
            int perPage = GetPageCount(itemsPerPage);

            var (season, seasonYear) = GetCurrentSeason();

            // Season, Jahr und Pagination müssen Teil des Cache-Keys sein.
            var cacheKey =
                $"anilist:simulcast:{season}:{seasonYear}:{indexOfPage}:{perPage}";

            if (_cache.TryGetValue(
                cacheKey,
                out IReadOnlyList<AnimeSummary>? cached)
                && cached is not null)
            {
                return cached;
            }

            var variables = new
            {
                indexOfPage,
                perPage,
                season,
                seasonYear,
                sort = new[] { "POPULARITY_DESC" }
            };

            var data = await SendAsync<MediaPageData>(
                ApiQueries.CurrentSimulcast,
                variables,
                cancellationToken);

            IReadOnlyList<AnimeSummary> result =
                (data.Page?.Media ?? [])
                    .Where(media => media is not null)
                    .Select(media => MapToSummary(media!))
                    .ToList();

            _cache.Set(cacheKey, result, PopularCacheDuration);

            return result;
        }

        private async Task<T> SendAsync<T>(string query, object? variables, CancellationToken cancellationToken)
            where T : class
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsJsonAsync(
                    string.Empty,
                    new GraphQlRequest(query, variables),
                    JsonOptions,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "API war nicht erreichbar.");
                throw new ApiException("API ist derzeit nicht erreichbar.", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Zeitüberschreitung bei der API-Anfrage.");
                throw new ApiException("Die Anfrage an API hat zu lange gedauert.", ex);
            }

            using (response)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    _logger.LogWarning("API-Rate-Limit erreicht, Retry-After: {RetryAfter}", retryAfter);
                    throw new ApiRateLimitException(retryAfter);
                }

                GraphQlResponse<T>? envelope;
                try
                {
                    envelope = await response.Content.ReadFromJsonAsync<GraphQlResponse<T>>(JsonOptions, cancellationToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Die API-Antwort konnte nicht gelesen werden (Status {Status}).", (int)response.StatusCode);
                    throw new ApiException("Die Antwort von API war ungültig.", ex);
                }

                var firstError = envelope?.Errors?.FirstOrDefault();
                if (firstError?.Status == 429)
                {
                    throw new ApiRateLimitException(response.Headers.RetryAfter?.Delta);
                }

                if (!response.IsSuccessStatusCode || firstError is not null || envelope?.Data is null)
                {
                    _logger.LogWarning(
                        "API-Anfrage fehlgeschlagen: Status {Status}, Meldung {Message}",
                        (int)response.StatusCode,
                        firstError?.Message);
                    throw new ApiException("API hat die Anfrage nicht erfolgreich beantwortet.");
                }

                return envelope.Data;
            }
        }

        private static AnimeSummary MapToSummary(MediaDto media)
        {
            var title = FirstNonEmpty(
                media.Title?.English,
                media.Title?.Romaji,
                media.Title?.UserPreferred,
                media.Title?.Native) ?? "Unbekannter Titel";

            var cover = FirstNonEmpty(
                media.CoverImage?.ExtraLarge,
                media.CoverImage?.Large,
                media.CoverImage?.Medium);

            string? format = null;

            if (!string.IsNullOrWhiteSpace(media.Format))
            {
                format = FormatLabels.TryGetValue(media.Format, out var label)
                    ? label
                    : media.Format;
            }

            var airing = media.NextAiringEpisode;

            DateTimeOffset? airingAt = airing?.AiringAt is int timestamp
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
                : null;

            TimeSpan? timeUntilAiring = airing?.TimeUntilAiring is int seconds
                ? TimeSpan.FromSeconds(seconds)
                : null;

            return new AnimeSummary(
                media.Id,
                title,
                cover,
                format,
                media.AverageScore,
                media.Popularity)
            {
                NextEpisodeNumber = airing?.Episode,
                NextEpisodeAiringAt = airingAt,
                TimeUntilNextEpisode = timeUntilAiring
            };
        }

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        private static int GetPageCount(int count)
        {
            return Math.Clamp(count, MIN_PAGE_NUMBER, MAX_PAGE_NUMBER);
        }

        private static int GetPageIndex(int pageIndex)
        {
            return Math.Max(MIN_PAGE_NUMBER, pageIndex);
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
