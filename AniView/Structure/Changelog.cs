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
            new(new DateOnly(2026, 10, 16),
                "Filterung und Sortierung",
                "In den Tabellen gibt es jetzt die Möglichkeit einer Filterung und Sortierung."),
            new(new DateOnly(2026, 10, 11),
                "Mehr Informationen zu jedem Anime",
                "In Simulcast und Top 10 öffnet das Info-Symbol oben rechts im Bild alle Details zu einem Anime, bei All Animes ein Klick auf die Zeile. Die Informationen werden erst beim Öffnen geladen."),
            new(new DateOnly(2026, 10, 11),
                "Beliebtheit in den Top 10",
                "Die Top 10 zeigen jetzt auch, wie beliebt ein Anime ist. Bei beendeten Animes steht statt der nächsten Folge, wie viele Folgen es gibt."),
            new(new DateOnly(2026, 10, 11),
                "Neue Startseite",
                "Hier siehst du zuerst die Neuigkeiten und darunter die zehn beliebtesten Animes."),
            new(new DateOnly(2026, 10, 11),
                "Neue Seite All Animes",
                "Auf dieser Seite sind nun alle Animes in tabellarischer Auflistung zu finden, auch mit der Anzahl ihrer Folgen."),
            new(new DateOnly(2026, 10, 11),
                "Simulcast mit Gesamtzahl",
                "Auf der Simulcast-Seite siehst du jetzt, wie viele Animes in der aktuellen Season laufen."),
            new(new DateOnly(2026, 10, 11),
                "Mehr Platz auf allen Seiten",
                "Die Seiten nutzen jetzt die volle Breite deines Bildschirms.")
        ];
    }
}
