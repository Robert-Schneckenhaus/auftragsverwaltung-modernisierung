# ADR 0003: Sicherheits-Baseline für das moderne Backend

**Status:** Akzeptiert (umgesetzt, Feature-Flag noch aus)

## Kontext
Das moderne Backend übernimmt bisher den API-Vertrag des Legacy-Systems unverändert (siehe Charakterisierungstests). Dazu gehören auch die Sicherheitsmängel aus dem [Altlasten-Katalog](../../legacy/README.md#altlasten-katalog):

- Alle Endpunkte sind ohne Anmeldung erreichbar, das „Token“ wird nie geprüft
- Rollen (z. B. Rechnungen nur für Admins) werden nur im Frontend durchgesetzt
- Passwörter liegen im Klartext in `tblBenutzer`

Diese Mängel einfach zu übernehmen ist keine Option. Sie sofort zu beheben würde aber das AngularJS-Frontend brechen, das während der Migration weiter genutzt wird.

## Entscheidung
1. **Authentifizierung über Cookie** (ASP.NET Core Cookie Authentication, `HttpOnly`, `SameSite=Strict`). Das AngularJS-Frontend muss dafür nicht angepasst werden, weil der Browser das Cookie automatisch mitsendet.
2. **Login-Endpunkt `api/login` wird als nächstes Modul migriert**, liefert weiterhin `Login`, `Rolle` und `Token` (für die Anzeige im Frontend) und setzt zusätzlich das Cookie.
3. **Passwörter werden beim ersten erfolgreichen Login transparent auf einen Hash umgestellt** (ASP.NET Core `PasswordHasher`). Eine neue Spalte `PasswortHash` wird ergänzt, die Klartextspalte danach geleert. Das ist die einzige Schemaänderung vor Abschaltung des Legacy-Backends und bricht ADR 0002 bewusst.
4. **Autorisierung serverseitig** per Policy: alle `api/*`-Endpunkte erfordern Anmeldung, `api/rechnungen` erfordert die Rolle `Admin`.
5. **Umstellung per Feature-Flag** (`Sicherheit:AnmeldungErforderlich`), damit die Charakterisierungstests bis zur Umstellung unverändert grün bleiben und danach gezielt angepasst werden.

## Umsetzung

| Baustein | Ort |
|---|---|
| Hash-Migration beim Login | `modern/src/Auftragsverwaltung.Domain/Anmeldung/Benutzer.cs` |
| Hashverfahren (ASP.NET Core `PasswordHasher`, PBKDF2) | `modern/src/Auftragsverwaltung.Api/Sicherheit/PasswortHasher.cs` |
| Cookie, Policies, Feature-Flag | `modern/src/Auftragsverwaltung.Api/Sicherheit/SicherheitExtensions.cs` |
| Schemaänderung | `modern/Datenbank/001_passwort_hash.sql` |
| Charakterisierungstests Login (Legacy und modern) | `modern/tests/Auftragsverwaltung.Charakterisierung/LoginTests.cs` |
| Sicherheitstests (nur modern, Flag an) | `modern/tests/Auftragsverwaltung.Sicherheitstests` |

Die CI startet die API zweimal: einmal wie das Legacy-System (Charakterisierungstests) und einmal mit `Sicherheit__AnmeldungErforderlich=true` (Sicherheitstests). Danach prüft sie in der Datenbank, dass für angemeldete Benutzer kein Klartextpasswort mehr existiert.

## Konsequenzen
- Solange das Flag aus ist, bleibt das bekannte Risiko bestehen; das muss mit dem Kunden zeitlich begrenzt vereinbart werden
- **Das Flag schützt nur Endpunkte, die bereits migriert sind.** Aufträge und Rechnungen laufen weiterhin über das Legacy-Backend und bleiben offen, bis sie migriert sind oder der Proxy die Anmeldung selbst prüft (siehe nächste Schritte)
- **Bewusste Verhaltensänderung:** Passwörter werden mit Groß-/Kleinschreibung geprüft. Im Legacy-System funktionierte z. B. `ADMIN` statt `admin`, weil der Vergleich in SQL stattfand
- Benutzer, die sich nie anmelden, behalten ihr Klartextpasswort. Vor Abschaltung des Legacy-Systems müssen diese Konten zurückgesetzt werden
- Das Legacy-Login funktioniert für umgestellte Benutzer nicht mehr. Unkritisch, weil `api/login` über den Proxy bereits vom modernen Backend bedient wird
- Das AngularJS-Frontend merkt sich den Login in `localStorage`, das Cookie läuft nach 8 Stunden Inaktivität ab. Danach zeigt das alte Frontend Fehler statt der Anmeldeseite. Wird mit dem Angular-Frontend (Phase 4) behoben
- Fehlerdetails (Stacktraces) werden im modernen Backend grundsätzlich nicht ausgeliefert; das ändert keinen dokumentierten Vertrag

## Nächste Schritte
1. Proxy prüft das Anmelde-Cookie auch für Legacy-Routen (gemeinsamer Data-Protection-Schlüssel für API und Proxy). Danach kann das Flag in Produktion eingeschaltet werden
2. Policy `NurAdmin` auf `api/rechnungen` anwenden, sobald das Modul migriert ist

## Offene Fragen an den Fachbereich
- Sollen Schweizer Postleitzahlen (4-stellig) erlaubt werden? Aktuell lehnt die Validierung sie ab.
- Soll der Status von Aufträgen auf feste Werte normalisiert werden (`offen` → `Offen`)?
