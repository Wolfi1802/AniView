using AniView.Models;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Services;
using Microsoft.AspNetCore.Components;

namespace AniView.Components.Pages
{
    public partial class Simulcast
    {
        // TODO soll der user einstellen dürfen
        private const int PageSize = 20;

        private string? _seasonString;

        [Inject]
        private ICurrentSimulcastService simulcastAnimes { get; set; } = default!;

        protected override void OnInitialized() => _seasonString = GetCurrentSeasonString();

        private Task<PageResult<AnimeSummary>> LoadPageAsync(int page, int pageSize, CancellationToken token) =>
            simulcastAnimes.GetCurrentSimulcastPageAsync(page, pageSize, token);

        private static string? GetErrorMessage(Exception exception) => exception switch
        {
            ApiRateLimitException { RetryAfter: { } wait } =>
                $"API bittet um etwas Geduld. Bitte versuche es in ca. {Math.Ceiling(wait.TotalSeconds)} Sekunden erneut.",
            ApiRateLimitException =>
                "API bittet um etwas Geduld. Bitte versuche es gleich noch einmal.",
            _ => null
        };

        private string GetCurrentSeasonString()
        {
            var now = DateTime.Now;

            string season = now.Month switch
            {
                >= 1 and <= 3 => $"Winter - {now.Year}",
                >= 4 and <= 6 => $"Fühling - {now.Year}",
                >= 7 and <= 9 => $"Sommer - {now.Year}",
                _ => $"Herbst - {now.Year}"
            };

            return season;
        }
    }
}
