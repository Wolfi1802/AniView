using AniView.Models;

namespace AniView.Structure
{
    /// <summary>
    /// Neuigkeiten zur Anwendung für den Bereich "What's new" der Startseite. Neueste Einträge zuerst.
    /// Neuer Eintrag: Id hier ergänzen und die Schlüssel "Changelog.{Id}.Title" und "Changelog.{Id}.Description"
    /// in allen Dateien unter Resources/SharedResource*.resx anlegen.
    /// </summary>
    public static class Changelog
    {
        public static IReadOnlyList<ChangelogEntry> Entries { get; } =
        [
            new(new DateOnly(2026, 10, 20), "AccentColor"),
            new(new DateOnly(2026, 10, 20), "Discover"),
            new(new DateOnly(2026, 10, 18), "CardDesign"),
            new(new DateOnly(2026, 10, 18), "ExtendedFilters"),
            new(new DateOnly(2026, 10, 18), "FullSimulcast"),
            new(new DateOnly(2026, 10, 16), "FilterSort"),
            new(new DateOnly(2026, 10, 11), "MoreInfo"),
            new(new DateOnly(2026, 10, 11), "TopTenPopularity"),
            new(new DateOnly(2026, 10, 11), "NewHome"),
            new(new DateOnly(2026, 10, 11), "NewAllAnimes"),
            new(new DateOnly(2026, 10, 11), "SimulcastCount"),
            new(new DateOnly(2026, 10, 11), "MorePadding")
        ];
    }
}
