using AniView.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AniView.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
        private MudThemeProvider? _themeProvider;
        private bool _isDarkMode;

        [Inject]
        private ThemeService ThemeService { get; set; } = default!;

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

        private async Task OnDarkModeChangedAsync(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
            await ThemeService.StorePreferenceAsync(isDarkMode);
        }
    }
}
