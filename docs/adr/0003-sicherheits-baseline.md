# ADR 0003: Sicherheits-Baseline für das moderne Backend

**Status:** Vorgeschlagen

## Kontext
Das moderne Backend übernimmt bisher den API-Vertrag des Legacy-Systems unverändert (siehe Charakterisierungstests). Dazu gehören auch die Sicherheitsmängel aus dem [Altlasten-Katalog](../../legacy/README.md#altlasten-katalog):

- Alle Endpunkte sind ohne Anmeldung erreichbar, das „Token“ wird nie geprüft
- Rollen (z. B. Rechnungen nur für Admins) werden nur im Frontend durchgesetzt
- Passwörter liegen im Klartext in `tblBenutzer`

Diese Mängel einfach zu übernehmen ist keine Option. Sie sofort zu beheben würde aber das AngularJS-Frontend brechen, das während der Migration weiter genutzt wird.

## Entscheidung (Vorschlag)
1. **Authentifizierung über Cookie** (ASP.NET Core Cookie Authentication, `HttpOnly`, `SameSite=Strict`). Das AngularJS-Frontend muss dafür nicht angepasst werden, weil der Browser das Cookie automatisch mitsendet.
2. **Login-Endpunkt `api/login` wird als nächstes Modul migriert**, liefert weiterhin `Login`, `Rolle` und `Token` (für die Anzeige im Frontend) und setzt zusätzlich das Cookie.
3. **Passwörter werden beim ersten erfolgreichen Login transparent auf einen Hash umgestellt** (ASP.NET Core `PasswordHasher`). Eine neue Spalte `PasswortHash` wird ergänzt, die Klartextspalte danach geleert. Das ist die einzige Schemaänderung vor Abschaltung des Legacy-Backends und bricht ADR 0002 bewusst.
4. **Autorisierung serverseitig** per Policy: alle `api/*`-Endpunkte erfordern Anmeldung, `api/rechnungen` erfordert die Rolle `Admin`.
5. **Umstellung per Feature-Flag** (`Sicherheit:AnmeldungErforderlich`), damit die Charakterisierungstests bis zur Umstellung unverändert grün bleiben und danach gezielt angepasst werden.

## Konsequenzen
- Solange das Flag aus ist, bleibt das bekannte Risiko bestehen; das muss mit dem Kunden zeitlich begrenzt vereinbart werden
- Charakterisierungstests erhalten einen Login-Schritt, sobald das Flag aktiv ist
- Fehlerdetails (Stacktraces) werden im modernen Backend grundsätzlich nicht ausgeliefert; das ändert keinen dokumentierten Vertrag

## Offene Fragen an den Fachbereich
- Sollen Schweizer Postleitzahlen (4-stellig) erlaubt werden? Aktuell lehnt die Validierung sie ab.
- Soll der Status von Aufträgen auf feste Werte normalisiert werden (`offen` → `Offen`)?
