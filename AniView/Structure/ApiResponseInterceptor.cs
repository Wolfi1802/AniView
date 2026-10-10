namespace AniView.Structure
{
    /// <summary>
    /// Protokolliert nicht erfolgreiche HTTP-Antworten externer APIs.
    /// </summary>
    public class ApiResponseInterceptor : DelegatingHandler
    {
        private readonly ILogger<ApiResponseInterceptor> _logger;

        public ApiResponseInterceptor(ILogger<ApiResponseInterceptor> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "API-Fehler: {StatusCode} {ReasonPhrase} – {Uri}",
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    request.RequestUri);
            }

            return response;
        }
    }
}
