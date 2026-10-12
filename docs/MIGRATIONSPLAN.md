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

- [ ] Phase 0: Bestandsaufnahme
- [ ] Phase 1: Sicherheitsnetz
- [ ] Phase 2: Fundament
- [ ] Phase 3: Backend
- [ ] Phase 4: Frontend
- [ ] Phase 5: Abschluss
