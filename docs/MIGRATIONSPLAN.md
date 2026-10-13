# Migrationsplan

Ziel: Die Auftragsverwaltung von .NET Framework 4.8 / AngularJS auf aktuelles .NET (LTS) und Angular bringen. Die Anwendung bleibt in jeder Phase lauffähig.

## Ausgangslage (`legacy/`)

Bewusst realistisch gebaut, mit typischen Altlasten:

- ASP.NET Web API 2 auf .NET Framework 4.8, EF6 auf einem per SQL-Skript gepflegten Schema
- Geschäftslogik direkt in Controllern, keine Trennung von Schichten
- AngularJS 1.x mit `$scope`, globalen Funktionen und jQuery-Resten
- Secrets in `Web.config`, Klartext-Passwörter, keine serverseitige Authentifizierung
- Keine automatisierten Tests

**Fachliche Module:** Kunden · Aufträge · Positionen/Material · Rechnungen · Benutzer

Der vollständige [Altlasten-Katalog](../legacy/README.md#altlasten-katalog) mit allen Sicherheits-, Architektur- und fachlichen Eigenheiten steht im Legacy-README.

## Phasen

### Phase 0: Bestandsaufnahme
- Abhängigkeiten und bekannte Sicherheitslücken erfassen
- Module, Endpunkte und Datenmodell dokumentieren
- Risiken und Reihenfolge festlegen
- **Ergebnis:** Inventarliste, priorisierter Backlog

### Phase 1: Sicherheitsnetz
- Charakterisierungstests gegen die bestehende API: Sie halten fest, *wie sich das System heute verhält*, nicht wie es sich verhalten sollte
- Snapshot der Datenbank als Testbasis
- **Ergebnis:** Tests laufen grün gegen `legacy/` und später unverändert gegen `modern/`

### Phase 2: Fundament
- Neue Solution: `Api`, `Domain`, `Infrastructure`, `tests`
- EF Core mit dem bestehenden Schema (kein Datenbank-Umbau in diesem Schritt)
- CI-Pipeline: Build, Tests, Abhängigkeits-Scan
- Reverse Proxy (YARP) vor beiden Backends, siehe [ADR 0001](adr/0001-strangler-fig.md)

### Phase 3: Backend Modul für Modul
Reihenfolge nach Risiko, das einfachste Modul zuerst:
1. Kunden
2. Aufträge
3. Positionen/Material
4. Rechnungen
5. Benutzer und Authentifizierung

Pro Modul: Endpunkte nachbauen → Charakterisierungstests grün → Route im Proxy umschalten → Legacy-Endpunkt entfernen.

### Phase 4: Frontend
- Neues Angular-Projekt mit Standalone Components und Signals
- Seiten in derselben Reihenfolge wie das Backend migrieren
- Übergangsweise laufen AngularJS- und Angular-Seiten parallel hinter dem Proxy

### Phase 5: Abschluss
- Legacy-Code entfernen, Proxy-Regeln vereinfachen
- Container-Build, Deployment der Demo
- Kennzahlen messen und im README eintragen

## KI-gestützte Arbeitsweise

Claude Code wird als Werkzeug eingesetzt, nicht als Autopilot.

**Wofür:**
- Analyse des Legacy-Codes und Erstellen der Inventarliste
- Entwürfe für Charakterisierungstests
- Umschreiben von Controllern, DTOs und Mappings nach festgelegtem Muster
- AngularJS-Templates in Angular-Komponenten übertragen

**Leitplanken:**
- Regeln für das Projekt stehen in [`CLAUDE.md`](../CLAUDE.md)
- Jede Änderung läuft durch die Tests und durch mein Code-Review
- Architektur- und Sicherheitsentscheidungen treffe ich selbst und halte sie als ADR fest
- Keine echten Kundendaten in Prompts

**Dokumentation:** Pro Phase wird kurz festgehalten, was KI-gestützt erledigt wurde, wie lange es gedauert hat und wo nachgearbeitet werden musste.

## Fortschritt

- [x] **Phase 0: Bestandsaufnahme**: API-Übersicht und Altlasten-Katalog in [`legacy/README.md`](../legacy/README.md)
- [ ] **Phase 1: Sicherheitsnetz**
  - [x] Charakterisierungstests Modul Kunden (19 Tests, `modern/tests/Auftragsverwaltung.Charakterisierung`)
  - [x] Grün gegen `modern/` (CI)
  - [ ] Grün gegen `legacy/` (lokal unter Windows, siehe unten)
  - [ ] Module Aufträge, Rechnungen, Login
- [x] **Phase 2: Fundament**: Solution, EF Core auf Legacy-Schema, CI mit SQL Server, YARP-Proxy
- [ ] **Phase 3: Backend**
  - [x] Kunden: migriert, Proxy-Route `api/kunden` zeigt auf `modern/`
  - [ ] Aufträge und Positionen
  - [ ] Rechnungen
  - [ ] Benutzer und Authentifizierung (Vorschlag: vorziehen, siehe [ADR 0003](adr/0003-sicherheits-baseline.md))
- [ ] **Phase 4: Frontend**
- [ ] **Phase 5: Abschluss**

### Charakterisierungstests gegen das Legacy-System ausführen

1. Datenbank frisch anlegen (siehe [`legacy/README.md`](../legacy/README.md#starten-windows)) und Legacy-Anwendung in Visual Studio starten
2. Tests ausführen:
   ```bat
   set AV_BASE_URL=http://localhost:51400/
   dotnet test modern\tests\Auftragsverwaltung.Charakterisierung
   ```
3. Danach Datenbank erneut frisch anlegen und dieselben Tests gegen die moderne API laufen lassen (`AV_BASE_URL=http://localhost:5080/`)

Ohne `AV_BASE_URL` werden die Tests übersprungen.

### Strangler-Proxy lokal ausprobieren

Legacy-Anwendung (Port 51400), moderne API (`dotnet run --project modern/src/Auftragsverwaltung.Api`, Port 5080) und Proxy (`dotnet run --project modern/src/Auftragsverwaltung.Proxy`, Port 5000) starten, dann http://localhost:5000 öffnen. Das alte AngularJS-Frontend läuft unverändert, die Kundenverwaltung wird aber bereits vom modernen Backend bedient.
