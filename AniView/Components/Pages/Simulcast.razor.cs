using AniView.Models;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AniView.Components.Pages
{
    public partial class Simulcast
    {
        // TODO soll der user einstellen dürfen
        private const int PageSize = 50;
        private const int PageIndex = 1;

        private readonly CancellationTokenSource _cts = new();

        private IReadOnlyList<AnimeSummary>? _items;
        private bool _isLoading = true;
        private string? _errorTitle;
        private string? _errorMessage;
        private string _errorIcon = Icons.Material.Filled.ErrorOutline;
        private string? _seasonString;

        [Inject]
        private ICurrentSimulcastService simulcastAnimes { get; set; } = default!;

        [Inject]
        private ILogger<Home> Logger { get; set; } = default!;

        protected override Task OnInitializedAsync() => LoadAll();


        private async Task LoadAll()
        {
            _seasonString = GetCurrentSeasonString();
            await LoadApiData();
        }

        private async Task LoadApiData()
        {
            _isLoading = true;
            _errorTitle = null;
            _errorMessage = null;

            try
            {
                _items = await simulcastAnimes.GetCurrentSimulcast(PageIndex, PageSize, _cts.Token);
                // TODO exception handling doppelt
            }
            catch (ApiRateLimitException ex)
            {
                _errorIcon = Icons.Material.Filled.HourglassEmpty;
                _errorTitle = "Zu viele Anfragen";
                _errorMessage = ex.RetryAfter is { } wait
                    ? $"API bittet um etwas Geduld. Bitte versuche es in ca. {Math.Ceiling(wait.TotalSeconds)} Sekunden erneut."
                    : "API bittet um etwas Geduld. Bitte versuche es gleich noch einmal.";
            }
            catch (ApiException ex)
            {
                Logger.LogWarning(ex, "Das Ranking konnte nicht geladen werden.");
                _errorIcon = Icons.Material.Filled.ErrorOutline;
                _errorTitle = "Das Ranking konnte nicht geladen werden";
                _errorMessage = "Bitte prüfe deine Verbindung und versuche es erneut.";
            }
            catch (OperationCanceledException)
            {
                // Komponente wurde verlassen, es gibt nichts mehr anzuzeigen.
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

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
