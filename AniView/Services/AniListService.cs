using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AniView.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Services
{
    public sealed class AniListService : IAniListService
    {
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
        private readonly ILogger<AniListService> _logger;

        public AniListService(HttpClient http, IMemoryCache cache, ILogger<AniListService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AnimeSummary>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
        {
            var perPage = Math.Clamp(count, 1, 50);
            var cacheKey = $"anilist:popular:{perPage}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AnimeSummary>? cached) && cached is not null)
            {
                return cached;
            }

            var variables = new { page = 1, perPage, sort = new[] { "POPULARITY_DESC" } };
            var data = await SendAsync<MediaPageData>(AniListQueries.MediaPage, variables, cancellationToken);

            IReadOnlyList<AnimeSummary> result = (data.Page?.Media ?? [])
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
                _logger.LogWarning(ex, "AniList war nicht erreichbar.");
                throw new AniListException("AniList ist derzeit nicht erreichbar.", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Zeitüberschreitung bei der AniList-Anfrage.");
                throw new AniListException("Die Anfrage an AniList hat zu lange gedauert.", ex);
            }

            using (response)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta;
                    _logger.LogWarning("AniList-Rate-Limit erreicht, Retry-After: {RetryAfter}", retryAfter);
                    throw new AniListRateLimitException(retryAfter);
                }

                GraphQlResponse<T>? envelope;
                try
                {
                    envelope = await response.Content.ReadFromJsonAsync<GraphQlResponse<T>>(JsonOptions, cancellationToken);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Die AniList-Antwort konnte nicht gelesen werden (Status {Status}).", (int)response.StatusCode);
                    throw new AniListException("Die Antwort von AniList war ungültig.", ex);
                }

                var firstError = envelope?.Errors?.FirstOrDefault();
                if (firstError?.Status == 429)
                {
                    throw new AniListRateLimitException(response.Headers.RetryAfter?.Delta);
                }

                if (!response.IsSuccessStatusCode || firstError is not null || envelope?.Data is null)
                {
                    _logger.LogWarning(
                        "AniList-Anfrage fehlgeschlagen: Status {Status}, Meldung {Message}",
                        (int)response.StatusCode,
                        firstError?.Message);
                    throw new AniListException("AniList hat die Anfrage nicht erfolgreich beantwortet.");
                }

                return envelope.Data;
            }
        }

        private static AnimeSummary MapToSummary(MediaDto media)
        {
            var title = FirstNonEmpty(media.Title?.English, media.Title?.Romaji) ?? "Unbekannter Titel";
            var cover = FirstNonEmpty(media.CoverImage?.ExtraLarge, media.CoverImage?.Large);

            string? format = null;
            if (!string.IsNullOrWhiteSpace(media.Format))
            {
                format = FormatLabels.TryGetValue(media.Format, out var label) ? label : media.Format;
            }

            return new AnimeSummary(media.Id, title, cover, format, media.AverageScore, media.Popularity);
        }

        private static string? FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
