# Legacy-Ausgangszustand

Auftragsverwaltung der (fiktiven) **Müller Haustechnik GmbH**, gebaut im Stil von etwa 2014:
ASP.NET Web API 2 auf .NET Framework 4.8, Entity Framework 6, AngularJS 1.x, Bootstrap 3, SQL Server.

Der Code enthält **absichtlich** typische Altlasten, damit die Modernisierung nachvollziehbar wird. Er ist nicht als Vorbild gedacht.

## Starten (Windows)

Voraussetzungen: Visual Studio 2022 mit Workload „ASP.NET und Webentwicklung“ (inkl. .NET Framework 4.8 und SQL Server LocalDB).

1. Datenbank anlegen:
   ```bat
   sqlcmd -S "(LocalDB)\MSSQLLocalDB" -f 65001 -i Datenbank\01_schema.sql
   sqlcmd -S "(LocalDB)\MSSQLLocalDB" -f 65001 -i Datenbank\02_testdaten.sql
   ```
2. `Auftragsverwaltung.sln` in Visual Studio öffnen, NuGet-Pakete werden beim Build wiederhergestellt
3. Mit F5 starten (IIS Express, http://localhost:51400/)
4. Anmelden mit `admin` / `admin`

## Funktionen

- **Kunden:** suchen, anlegen, bearbeiten, deaktivieren
- **Aufträge:** anlegen, Status setzen, Positionen erfassen
- **Rechnungen:** aus erledigten Aufträgen erzeugen, als bezahlt markieren
- **Login** mit Rollen (Admin, Mitarbeiter, Techniker)

## API

| Methode | Route | Zweck |
|---|---|---|
| POST | `api/login` | Anmeldung |
| GET | `api/kunden?suche=&nurAktive=` | Kundenliste |
| GET | `api/kunden/{id}` | Kunde mit Aufträgen |
| POST | `api/kunden` | Kunde anlegen |
| PUT | `api/kunden/{id}` | Kunde ändern |
| DELETE | `api/kunden/{id}` | Kunde deaktivieren |
| GET | `api/auftraege?status=&kdnr=` | Auftragsliste |
| GET | `api/auftraege/{id}` | Auftrag mit Positionen |
| POST | `api/auftraege` | Auftrag anlegen |
| PUT | `api/auftraege/{id}/status` | Status setzen |
| POST | `api/auftraege/{id}/positionen` | Position hinzufügen |
| DELETE | `api/auftraege/{id}/positionen/{posId}` | Position entfernen |
| POST | `api/auftraege/{id}/rechnung` | Rechnung erzeugen |
| GET | `api/rechnungen?offen=` | Rechnungsliste |
| GET | `api/rechnungen/{id}` | Rechnung mit Positionen |
| PUT | `api/rechnungen/{id}/bezahlt` | Als bezahlt markieren |

## Altlasten-Katalog

Diese Liste ist die Grundlage für die Modernisierung. Jede Altlast wird im modernen Teil behoben und im Migrationsplan abgehakt.

**Sicherheit**
- Passwörter im Klartext in der Datenbank
- Passwortvergleich in SQL ohne Beachtung der Groß-/Kleinschreibung (`ADMIN` funktioniert statt `admin`)
- „Token“ ist nur Base64 aus Login und Rolle und wird serverseitig nie geprüft, alle API-Endpunkte sind ohne Anmeldung erreichbar
- Rechteprüfung (Rechnungen nur für Admins) nur im Frontend
- Secrets im Klartext in `Web.config` (SMTP-Passwort, auskommentierter `sa`-Zugang)
- Fehlerdetails inkl. Stacktrace werden an den Client ausgegeben
- jQuery 1.11 und AngularJS ohne Support

**Architektur**
- Geschäftslogik direkt in Controllern (z. B. Rechnungsberechnung, Statusregeln)
- `DbContext` als Feld, wird nie freigegeben
- Anonyme Objekte als API-Antworten, keine DTOs, kein API-Vertrag
- MwSt-Satz fest im Code

**Fachliche Eigenheiten (müssen bei der Migration bewusst entschieden werden)**
- Status ist Freitext, in den Testdaten existiert `offen` neben `Offen`. Der Filter findet beides nur, weil SQL Server standardmäßig nicht zwischen Groß- und Kleinschreibung unterscheidet
- `Math.Round` nutzt Banker's Rounding (Rundung auf gerade Zahl), nicht kaufmännisches Runden
- Statusprüfungen im Code unterscheiden dagegen Groß- und Kleinschreibung, das Verhalten hängt also davon ab, ob in der Datenbank oder im Speicher verglichen wird

**Frontend**
- `$scope`-Controller, globale Funktionen, DOM-Manipulation per jQuery
- Login-Zustand in `localStorage`, Seitenschutz nur clientseitig
- Status „In Arbeit“ erhält wegen des Leerzeichens keine Farbe

**Betrieb**
- Keine Tests, kein CI, manuelles Deployment
