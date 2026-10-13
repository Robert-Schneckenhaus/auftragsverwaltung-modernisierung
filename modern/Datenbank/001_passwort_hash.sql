-- ADR 0003: Spalte für Passwort-Hashes.
-- Einzige Schemaänderung vor Abschaltung des Legacy-Backends. Das Legacy-System
-- ignoriert die Spalte; sein Login wird über den Proxy bereits vom modernen Backend bedient.
-- Idempotent, kann mehrfach ausgeführt werden.

USE Auftragsverwaltung;
GO

IF COL_LENGTH('tblBenutzer', 'PasswortHash') IS NULL
    ALTER TABLE tblBenutzer ADD PasswortHash NVARCHAR(200) NULL;
GO
