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

## Aktueller Stand

[![CI](https://github.com/Robert-Schneckenhaus/auftragsverwaltung-modernisierung/actions/workflows/ci.yml/badge.svg)](https://github.com/Robert-Schneckenhaus/auftragsverwaltung-modernisierung/actions/workflows/ci.yml)

- ✅ Legacy-Anwendung inkl. [Altlasten-Katalog](legacy/README.md#altlasten-katalog)
- ✅ Modernes Fundament: .NET 10, EF Core auf dem bestehenden Schema, YARP als Strangler-Proxy
- ✅ **Modul Kunden migriert**, abgesichert durch Charakterisierungstests, die in der CI gegen einen echten SQL Server laufen
- ✅ **Login und Sicherheits-Baseline**: Klartextpasswörter werden beim Login transparent gehasht, Cookie-Anmeldung, serverseitige Anmeldepflicht ([ADR 0003](docs/adr/0003-sicherheits-baseline.md)). Der Proxy schützt dabei auch die noch nicht migrierten Legacy-Endpunkte, abgesichert durch eigene Sicherheits- und Proxy-Tests
- ⏳ Aufträge, Rechnungen · Angular-Frontend

Details und nächste Schritte: [Migrationsplan](docs/MIGRATIONSPLAN.md#fortschritt)

## Projekt lokal starten

Voraussetzungen: .NET 10 SDK, SQL Server (LocalDB genügt), für `legacy/` zusätzlich Windows mit Visual Studio 2022.

```bash
# Datenbank anlegen (einmalig bzw. für frische Testdaten)
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -f 65001 -i legacy/Datenbank/01_schema.sql
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -f 65001 -i legacy/Datenbank/02_testdaten.sql
sqlcmd -S "(LocalDB)\MSSQLLocalDB" -f 65001 -i modern/Datenbank/001_passwort_hash.sql

# Modernes Backend (http://localhost:5080)
dotnet run --project modern/src/Auftragsverwaltung.Api

# Tests
dotnet test modern/tests/Auftragsverwaltung.Domain.Tests
AV_BASE_URL=http://localhost:5080/ dotnet test modern/tests/Auftragsverwaltung.Charakterisierung
```

Wie man das alte Frontend über den Proxy mit dem neuen Backend betreibt, steht im [Migrationsplan](docs/MIGRATIONSPLAN.md#strangler-proxy-lokal-ausprobieren).

## Struktur

```
legacy/      Ausgangszustand: .NET Framework 4.8 + AngularJS (Starten: legacy/README.md)
  Auftragsverwaltung.Web/   Web API 2, EF6, AngularJS-Frontend
  Datenbank/                SQL-Schema und Testdaten
modern/      Zielzustand: aktuelles .NET + Angular
  src/       Api, Domain (Fachlogik), Infrastructure (EF Core), Proxy (YARP), Sicherheit (gemeinsame Anmeldung)
  tests/     Domain-Tests, Charakterisierungstests (gegen legacy/ und modern/), Sicherheitstests
  Datenbank/ Schemaänderungen des modernen Systems
  web/       Angular-Frontend (folgt)
docs/        Migrationsplan, Architekturentscheidungen (ADRs), Screenshots
```

---

## Sie haben eine ähnliche Anwendung?

Ich unterstütze Unternehmen dabei, gewachsene **.NET- und Angular-Anwendungen** zu modernisieren, abzusichern und um KI-Funktionen zu erweitern, ohne Big-Bang-Neubau.

📫 **Kontakt:** _E-Mail / LinkedIn / Website eintragen_

_Hinweis: Das Unternehmen und alle Daten in diesem Projekt sind fiktiv._
