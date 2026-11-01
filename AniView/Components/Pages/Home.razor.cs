using AniView.Models;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AniView.Components.Pages
{
    public partial class Home
    {
        private const int RankingSize = 10;

        private readonly CancellationTokenSource _cts = new();

        private IReadOnlyList<AnimeSummary>? _items;
        private bool _isLoading = true;
        private Exception? _error;

        [Inject]
        private IPopularAnimeService popularAnimes { get; set; } = default!;

        [Inject]
        private ILogger<Home> Logger { get; set; } = default!;

        // The texts are built when rendering (not when the error happens), so they follow a language change.
        private string ErrorTitle => _error is ApiRateLimitException
            ? L["Home.Error.RateLimitTitle"]
            : L["Home.Error.LoadTitle"];

        private string ErrorMessage => _error is ApiRateLimitException rateLimit
            ? L.RateLimitMessage(rateLimit.RetryAfter)
            : L["Error.Connection"];

        private string ErrorIcon => _error is ApiRateLimitException
            ? Icons.Material.Filled.HourglassEmpty
            : Icons.Material.Filled.ErrorOutline;

        protected override Task OnInitializedAsync() => LoadAsync();

        private async Task LoadAsync()
        {
            _isLoading = true;
            _error = null;

            try
            {
                _items = await popularAnimes.GetPopularAsync(RankingSize, _cts.Token);
                // TODO exception handling doppelt
            }
            catch (ApiRateLimitException ex)
            {
                _error = ex;
            }
            catch (ApiException ex)
            {
                Logger.LogWarning(ex, "Das Ranking konnte nicht geladen werden.");
                _error = ex;
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
    }
}
