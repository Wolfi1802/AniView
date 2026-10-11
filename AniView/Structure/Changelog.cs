using AniView.Models;

namespace AniView.Structure
{
    /// <summary>
    /// Neuigkeiten zur Anwendung für den Bereich "What's new" der Startseite. Neueste Einträge zuerst.
    /// </summary>
    public static class Changelog
    {
        public static IReadOnlyList<ChangelogEntry> Entries { get; } =
        [
            new(new DateOnly(2026, 10, 11),
                "Neue Startseite",
                "Neuigkeiten stehen oben, darunter die Top 10 Animes."),
            new(new DateOnly(2026, 10, 11),
                "All Animes mit Infinite Scroll",
                "Die Tabelle lädt beim Scrollen automatisch weitere Animes nach und zeigt jetzt auch die Anzahl der Episoden."),
            new(new DateOnly(2026, 10, 11),
                "Simulcast zeigt die Gesamtanzahl",
                "Unter der Überschrift steht, wie viele Animes in der aktuellen Season laufen."),
            new(new DateOnly(2026, 10, 11),
                "Mehr Platz auf allen Seiten",
                "Der Inhalt nutzt die volle Breite mit einheitlichem Abstand zum Rand.")
        ];
    }
}
