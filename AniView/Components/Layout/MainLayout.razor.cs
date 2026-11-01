using AniView.Structure.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AniView.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        private MudThemeProvider? _themeProvider;
        private bool _isDarkMode;
        private bool _drawerOpen = true;

        [Inject]
        private ThemeService ThemeService { get; set; } = default!;

        // The layout derives from LayoutComponentBase, so it gets the language service explicitly.
        [Inject]
        private LanguageService L { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            L.LanguageChanged += OnLanguageChanged;

            // Without prerendering the browser is already connected, so the stored language is known before the first render.
            await L.InitializeAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            // Gespeicherte Wahl hat Vorrang, sonst gilt die Systemeinstellung.
            var stored = await ThemeService.LoadStoredPreferenceAsync();
            _isDarkMode = stored
                ?? (_themeProvider is not null && await _themeProvider.GetSystemDarkModeAsync());

            StateHasChanged();
        }

        private void ToggleDrawer() => _drawerOpen = !_drawerOpen;

        private async Task OnDarkModeChangedAsync(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
            await ThemeService.StorePreferenceAsync(isDarkMode);
        }

        private void OnLanguageChanged() => _ = InvokeAsync(StateHasChanged);

        public void Dispose() => L.LanguageChanged -= OnLanguageChanged;
    }
}
