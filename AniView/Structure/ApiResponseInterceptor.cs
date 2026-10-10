using System.Diagnostics;

namespace AniView.Structure
{
    public class ApiResponseInterceptor : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(
                    $"API-Fehler: {(int)response.StatusCode} " +
                    $"{response.StatusCode} – {request.RequestUri}");
            }

            return response;
        }
    }
}