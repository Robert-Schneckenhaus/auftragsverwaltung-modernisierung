# Projektregeln für Claude Code

## Ziel
Migration der Auftragsverwaltung von `legacy/` (.NET Framework 4.8, EF6, AngularJS) nach `modern/` (aktuelles .NET LTS, EF Core, Angular). Siehe `docs/MIGRATIONSPLAN.md`.

## Regeln
- `legacy/` wird nur gelesen, nicht verändert (Ausnahme: Charakterisierungstests anbinden)
- Verhalten der API bleibt identisch, bis ein ADR etwas anderes festlegt
- Geschäftslogik gehört nach `modern/src/Domain`, nicht in Controller
- Jede neue Funktion braucht Tests in `modern/tests`
- Angular: Standalone Components, Signals, keine NgModules, kein jQuery
- Keine Secrets im Code, Konfiguration über `appsettings` und Umgebungsvariablen
- Architekturentscheidungen nicht selbst treffen, sondern als Vorschlag mit ADR-Entwurf (`docs/adr/_vorlage.md`) vorlegen

## Ablauf pro Modul
1. Endpunkte im Legacy-Code analysieren und auflisten
2. Charakterisierungstests schreiben, gegen `legacy/` grün
3. Im modernen Backend nachbauen, Tests gegen `modern/` grün
4. Proxy-Route umschalten
5. Fortschritt in `docs/MIGRATIONSPLAN.md` abhaken
