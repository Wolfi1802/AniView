using Microsoft.JSInterop;

namespace AniView.Structure.Services
{
    /// <summary>
    /// Speichert und liest die gewählte Farbvariante (hell/dunkel) im localStorage des Browsers.
    /// Darf nur nach dem ersten Rendern aufgerufen werden (JS-Interop).
    /// </summary>
    public sealed class ThemeService
    {
        private const string StorageKey = "aniview-theme";

        private readonly IJSRuntime _js;
        private readonly ILogger<ThemeService> _logger;

        public ThemeService(IJSRuntime js, ILogger<ThemeService> logger)
        {
            _js = js;
            _logger = logger;
        }

        /// <summary>
        /// Liefert true für dunkel, false für hell und null, wenn nichts gespeichert ist oder nicht gelesen werden konnte.
        /// </summary>
        public async Task<bool?> LoadStoredPreferenceAsync()
        {
            try
            {
                var value = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                return value switch
                {
                    "dark" => true,
                    "light" => false,
                    _ => null
                };
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                _logger.LogDebug(ex, "Theme-Einstellung konnte nicht gelesen werden.");
                return null;
            }
        }

        public async Task StorePreferenceAsync(bool isDarkMode)
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, isDarkMode ? "dark" : "light");
            }
            catch (Exception ex) when (IsRecoverable(ex))
            {
                _logger.LogDebug(ex, "Theme-Einstellung konnte nicht gespeichert werden.");
            }
        }

        private static bool IsRecoverable(Exception ex) =>
            ex is JSException or JSDisconnectedException or InvalidOperationException or TaskCanceledException;
    }
}
