using MudBlazor;

namespace AniView.Theme
{
    /// <summary>
    /// Zentrales Erscheinungsbild der Anwendung (heller und dunkler Modus).
    /// Alle Farben und Abstände werden nur hier festgelegt.
    /// </summary>
    public static class AniViewTheme
    {
        public static MudTheme Default { get; } = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0E7C86",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#6D4AFF",
                Background = "#F4F6F9",
                Surface = "#FFFFFF",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#1B2733",
                TextPrimary = "#1B2733",
                TextSecondary = "#4A5A6A",
                LinesDefault = "#DCE2E9"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#4DD0C8",
                PrimaryContrastText = "#06201F",
                Secondary = "#A590FF",
                Background = "#0F1720",
                Surface = "#17212B",
                AppbarBackground = "#17212B",
                AppbarText = "#E6EDF3",
                TextPrimary = "#E6EDF3",
                TextSecondary = "#A9B7C4",
                LinesDefault = "#2A3846"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px"
            }
        };
    }
}
