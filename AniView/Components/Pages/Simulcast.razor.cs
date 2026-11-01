using AniView.Models;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Services;
using Microsoft.AspNetCore.Components;

namespace AniView.Components.Pages
{
    public partial class Simulcast
    {
        // TODO soll der user einstellen dürfen
        private const int ItemsPerPage = 20;

        private string _seasonCode = string.Empty;
        private int _seasonYear;
        private int? _totalItems;

        // Sortierung und Filter. Ein neuer Wert setzt die Tabelle zurück (ResetKey) und lädt ab Seite 1 neu.
        private AnimeListOptions _options = AnimeListOptions.SimulcastDefault;

        [Inject]
        private ICurrentSimulcastService simulcastAnimes { get; set; } = default!;

        // The season is kept as an API code and translated when rendering, so it follows a language change.
        private string SeasonText => $"{L.Label("Season", _seasonCode)} - {_seasonYear}";

        private string SubTitle => _totalItems is { } total
            ? $"{SeasonText} - {L.Plural("Simulcast.Count", total)}"
            : SeasonText;

        protected override void OnInitialized() => (_seasonCode, _seasonYear) = GetCurrentSeason();

        private Task<PageResult<AnimeSummary>> LoadPageAsync(int page, int pageSize, CancellationToken token) =>
            simulcastAnimes.GetCurrentSimulcastPageAsync(page, pageSize, _options, token);

        private string? GetErrorMessage(Exception exception) =>
            exception is ApiRateLimitException rateLimit ? L.RateLimitMessage(rateLimit.RetryAfter) : null;

        private static (string Code, int Year) GetCurrentSeason()
        {
            var now = DateTime.Now;

            var code = now.Month switch
            {
                >= 1 and <= 3 => "WINTER",
                >= 4 and <= 6 => "SPRING",
                >= 7 and <= 9 => "SUMMER",
                _ => "FALL"
            };

            return (code, now.Year);
        }
    }
}
