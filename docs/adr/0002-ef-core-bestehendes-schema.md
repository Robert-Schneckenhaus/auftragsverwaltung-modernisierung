# ADR 0002: EF Core auf dem bestehenden Datenbankschema

**Status:** Akzeptiert

## Kontext
Altes und neues Backend greifen während der Migration auf dieselbe Datenbank zu. Ein Schema-Umbau würde beide Seiten gleichzeitig betreffen.

## Entscheidung
EF Core wird per Scaffolding auf das bestehende Schema gesetzt. Schema-Verbesserungen folgen erst, wenn das Legacy-Backend abgeschaltet ist.

## Konsequenzen
- Kein Datenmigrationsrisiko während der Umstellung
- Unschöne Tabellen- und Spaltennamen bleiben vorerst, werden aber im Domain-Modell sauber gemappt
