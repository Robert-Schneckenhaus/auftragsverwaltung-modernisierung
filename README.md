# Auftragsverwaltung: Von Legacy zu modern

**Wie man eine gewachsene .NET-/AngularJS-Anwendung schnell und risikoarm auf aktuelles .NET und Angular bringt, ohne den laufenden Betrieb zu gefährden.**

> 🔗 **Live-Demo:** _folgt_ · 📄 **Migrationsplan:** [docs/MIGRATIONSPLAN.md](docs/MIGRATIONSPLAN.md) · 🧭 **Architekturentscheidungen:** [docs/adr](docs/adr)

<!-- Screenshot/GIF: Vorher (legacy) vs. Nachher (modern) nebeneinander -->
<!-- ![Vorher und Nachher](docs/screenshots/vorher-nachher.png) -->

---

## Das Problem

Viele mittelständische Unternehmen arbeiten täglich mit Software, die vor 10 bis 15 Jahren gebaut wurde:

- **.NET Framework 4.x und AngularJS** erhalten keine Sicherheitsupdates mehr
- Entwickler, die den Code kennen, sind nicht mehr im Haus
- Jede Änderung ist riskant, weil es keine automatisierten Tests gibt
- Ein kompletter Neubau ist teuer, dauert lange und legt den Betrieb lahm

## Die Lösung in diesem Projekt

Dieses Repository zeigt am Beispiel einer **Auftragsverwaltung für einen fiktiven Haustechnik-Betrieb**, wie eine solche Anwendung schrittweise modernisiert wird:

| | Vorher (`legacy/`) | Nachher (`modern/`) |
|---|---|---|
| Backend | ASP.NET Web API 2, .NET Framework 4.8 | ASP.NET Core, aktuelles .NET (LTS) |
| Datenzugriff | Entity Framework 6 | Entity Framework Core |
| Frontend | AngularJS 1.x | Angular (Standalone Components, Signals) |
| Tests | keine | Unit-, Integrations- und Charakterisierungstests |
| Auslieferung | manuelles Deployment | CI/CD mit GitHub Actions, Container |

**Vorgehen:** Erst Tests als Sicherheitsnetz, dann Migration Modul für Modul (Strangler-Fig-Pattern). Die Anwendung bleibt in jeder Phase lauffähig.

## Ergebnisse

<!-- Nur echte, gemessene Werte eintragen. -->

| Kennzahl | Vorher | Nachher |
|---|---|---|
| Testabdeckung | 0 % | _tbd_ |
| Ladezeit Auftragsliste | _tbd_ | _tbd_ |
| Bekannte Sicherheitslücken (Abhängigkeiten) | _tbd_ | _tbd_ |
| Aufwand der Migration | _tbd_ | |

## KI-gestützte Modernisierung

Ein Teil der Migration wurde mit **Claude Code** als Werkzeug durchgeführt, gesteuert und geprüft durch mich. Wie das konkret aussah und wo die Grenzen liegen, steht im [Migrationsplan](docs/MIGRATIONSPLAN.md#ki-gestützte-arbeitsweise).

Kurz gesagt: KI beschleunigt Fleißarbeit wie das Umschreiben von Controllern oder das Erzeugen von Tests. Architekturentscheidungen, Code-Review und fachliche Prüfung bleiben Handarbeit.

## Projekt lokal starten

```bash
# Modernes Backend
cd modern
dotnet run --project src/Api

# Modernes Frontend
cd modern/web
npm install
npm start
```

Details zu Voraussetzungen folgen, sobald die Implementierung steht.

## Struktur

```
legacy/      Ausgangszustand: .NET Framework 4.8 + AngularJS
modern/      Zielzustand: aktuelles .NET + Angular
  src/       Api, Domain, Infrastructure
  tests/     Automatisierte Tests
  web/       Angular-Frontend
docs/        Migrationsplan, Architekturentscheidungen (ADRs), Screenshots
```

---

## Sie haben eine ähnliche Anwendung?

Ich unterstütze Unternehmen dabei, gewachsene **.NET- und Angular-Anwendungen** zu modernisieren, abzusichern und um KI-Funktionen zu erweitern, ohne Big-Bang-Neubau.

📫 **Kontakt:** _E-Mail / LinkedIn / Website eintragen_

_Hinweis: Das Unternehmen und alle Daten in diesem Projekt sind fiktiv._
