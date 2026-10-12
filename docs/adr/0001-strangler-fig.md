# ADR 0001: Schrittweise Migration nach dem Strangler-Fig-Pattern

**Status:** Akzeptiert

## Kontext
Die Auftragsverwaltung ist im täglichen Einsatz. Ein kompletter Neubau mit Stichtag-Umstellung würde den Betrieb monatelang blockieren und das Risiko auf einen Tag bündeln.

## Entscheidung
Ein Reverse Proxy (YARP) sitzt vor dem alten und dem neuen Backend. Module werden einzeln migriert und per Routing-Regel umgeschaltet. Charakterisierungstests stellen sicher, dass sich das Verhalten nicht ändert.

## Konsequenzen
- Jede Phase ist einzeln auslieferbar und rückrollbar
- Übergangsweise laufen zwei Backends, das erhöht kurzzeitig den Betriebsaufwand
- Das bestehende Datenbankschema bleibt vorerst unverändert
