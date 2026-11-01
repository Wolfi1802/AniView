# CLAUDE.md – AniView

Diese Datei legt Scope und Arbeitsregeln für Claude in diesem Projekt fest. Sie hat Vorrang vor allgemeinen Gewohnheiten. Bei Widerspruch zu einer Anfrage: kurz nachfragen, nicht raten.

## 1. Projekt

AniView ist eine Anime-Entdeckungsseite mit eigenem Erscheinungsbild (kein AniList-Branding).

- Framework: Blazor Web App, .NET 10, Interaktivität **Server**, global
- Keine Authentifizierung, HTTPS
- UI: MudBlazor 9.11.0
- Datenquelle: AniList GraphQL API
- Pflichtfunktion: Filter nach Streaming-Anbieter (z. B. Crunchyroll)
- Deployment: Docker auf Render (https://wolfisblazorpage.onrender.com)
- Repo: https://github.com/Wolfi1802/AniView

## 2. Seiten (Stand jetzt)

| Route | Inhalt |
|---|---|
| `/` | Startseite: "What's new" (Changelog, nach Datum aufklappbar) + Top 10 Ranking |
| `/simulcast` | Aktuelle Season, Karten-Design, Infinite Scroll |
| `/overall-animes` | Alle Animes als Tabelle (inkl. Spalte "Episoden") |
| `/anime/{id}` | Detailseite |

> Bitte prüfen: Route für "Alle Animes" (`/overall-animes` vs. `/anime`).

## 3. Harte Regeln (Scope-Schutz)

Claude darf **ohne ausdrückliche Rückfrage und Freigabe nicht**:

1. **Pushen.** Niemals. Pushes macht ausschließlich der Entwickler. Committen ist erlaubt, aber nur lokal und nach den Regeln in Abschnitt 9.
2. **Neue NuGet-Pakete** hinzufügen. MudBlazor ist die UI-Bibliothek.
3. **Zusätzliche Features** bauen, die nicht ausdrücklich gewünscht wurden.
4. **Ungefragt refactoren.** Nur Dateien ändern, die zur Aufgabe gehören.
5. Die **Architektur ändern** (Blazor Server bleibt; kein WASM/Auto-Render, keine Authentifizierung).
6. Bestehende **Designentscheidungen** eigenmächtig ändern.

Erlaubt und erwünscht: Bei Auffälligkeiten **kurz anmerken**, was refactored werden sollte oder wo ein Bug entsteht. Nur anmerken, nicht umsetzen.

## 4. Arbeitsablauf

0. **Branch validieren (vor jeder Sitzung, vor jeder Änderung):** Aktuellen Branch per `git branch --show-current` ermitteln, nennen und mit dem Entwickler bestätigen ("Wir arbeiten in Branch X, ist das richtig für diese Aufgabe?"). Erst nach Bestätigung starten. Passt der Branch nicht zur Aufgabe, nicht selbst wechseln, sondern den Entwickler darauf hinweisen.
1. **Kurze Analyse** des Ziels und der betroffenen Dateien.
2. **Plan** zeigen (betroffene Dateien, Komponenten, ggf. GraphQL-Queries, offene Fragen).
3. **Freigabe abwarten** bei allem, was über eine kleine Korrektur hinausgeht.
4. **Schrittweise umsetzen** und am Ende zusammenfassen: geänderte/neue Dateien, was geprüft wurde, was noch offen ist.
5. Nicht Geprüftes (Build, Tests, Optik) klar als nicht geprüft kennzeichnen. Nichts als "funktioniert" melden, was nicht verifiziert wurde.

## 5. Architektur und Wiederverwendung

Vorhandene Bausteine zuerst nutzen, nicht neu bauen:

- **Infinite-Table-Komponente**: wiederverwendbar, Infinite Scroll mit Pagination im Hintergrund, Darstellung per Enum (z. B. Karten-Design); kennt `totalItems` und `itemsPerPage`. Kein Caching beim Hochscrollen.
- **LanguageService**: Übersetzung Deutsch/Englisch.
- **Config-Service**: zentrale Einstellungen (sichtbare Filter, sichtbare Infos). Unterstützt Setter; Besucher-UI folgt später. Bisherige Darstellungen sind die Defaults.
- Trennung: UI-Komponenten, Services, Datenzugriff (AniList). Keine Geschäftslogik im Code-behind. Keine blockierenden Aufrufe auf dem UI-Thread.

## 6. Sprache und Texte

- **Alle sichtbaren UI-Texte über den LanguageService** (DE/EN). Keine hartcodierten Texte in Komponenten.
- Code, Bezeichner und Kommentare auf Englisch. Antworten an den Entwickler auf Deutsch.

## 7. UI-Konventionen

- Seitenleiste links, Schriftzug oben links führt zur Startseite, Seiten erscheinen in der Mitte.
- Seiten-Padding links/rechts: 24 px auf allen Seiten.
- Tabellen-Header und Filter/Sortierung sind **immer sticky**; darunter dürfen keine Zeilen durchscheinen.
- Responsive über gängige Breakpoints. Top 10: 5 Karten pro Zeile, bei weniger Platz 2, auf dem Handy 1.
- Karten-Design: horizontal (Cover links, Infos rechts). Klick aufs Bild öffnet das Detail-Modal. Beim Nachladen Skeleton statt leerer Fläche.
- Sortierung "Erscheinung" basiert auf der Zeit bis zur nächsten Folge.
- Einheitliche Lade-, Leer- und Fehlerzustände.

## 8. Daten (AniList)

- Simulcast muss vollständig sein: Season-Liste mit AiringSchedule zusammenführen, inkl. noch nicht gestarteter Serien und Serien aus früheren Seasons.
- API-Antworten auf fehlende oder unerwartete Daten prüfen. Fehler und Rate Limits sauber behandeln.
- Keine Geheimnisse im Repo.

## 9. Branches

Ein Branch pro Feature (z. B. `card-design`, `config-service`).

- Claude arbeitet **immer im aktuell ausgecheckten Branch**. Kein Branch-Wechsel, kein neuer Branch, kein Merge durch Claude.
- Vor jeder Sitzung wird der Branch validiert (siehe Arbeitsablauf, Schritt 0).
- **Claude darf committen**, aber nur lokal, nur im aktuell ausgecheckten (und validierten) Branch und erst, wenn der Plan freigegeben und die Aufgabe umgesetzt ist. Erlaubte Git-Schreibbefehle: `git add` (nur die Dateien der Aufgabe, kein `git add -A` über fremde Änderungen) und `git commit`.
- **Pushes macht ausschließlich der Entwickler.** Claude führt kein `push` aus.
- Weiterhin verboten: `checkout`, `switch`, `merge`, `rebase`, `reset`, `stash`, `tag`, Branches anlegen oder löschen, `commit --amend` und jede Form von Force. Bereits vorhandene Commits werden nie verändert.
- Lesende Befehle wie `git status`, `git branch --show-current`, `git log` und `git diff` sind erlaubt.
- Pro abgeschlossener Aufgabe ein Commit mit aussagekräftiger Commit-Message (Betreff in einer Zeile, bei Bedarf Details). Claude nennt nach dem Commit Message und enthaltene Dateien, damit der Entwickler vor dem Push prüfen kann.
- Ist unklar, ob eine Änderung in den Commit gehört (zum Beispiel fremde, nicht zur Aufgabe passende Änderungen im Arbeitsverzeichnis), fragt Claude nach, statt sie mit einzuchecken.

## 10. Definition of Done

- Build ohne Fehler und Warnungen, die neu hinzugekommen sind
- Keine hartcodierten UI-Texte
- Layout geprüft für Desktop, Tablet und Handy
- Keine ungewollten Änderungen außerhalb des Scopes
- Zusammenfassung der Änderungen liegt vor
