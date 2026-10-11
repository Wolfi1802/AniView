using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using System.Net;
using System.Text.Json;

namespace AniView.Structure.API.Services
{
    public sealed class AniListClient : IAniListClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly ILogger<AniListClient> _logger;

        public AniListClient(HttpClient http, ILogger<AniListClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<T> SendAsync<T>(string query, object? variables, CancellationToken cancellationToken)
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
    }
}
