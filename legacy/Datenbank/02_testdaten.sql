-- Testdaten (alle Namen und Adressen sind fiktiv)
USE Auftragsverwaltung;
GO

INSERT INTO tblBenutzer (Login, Passwort, Rolle) VALUES
 ('admin',  'admin',     'Admin'),
 ('sabine', 'sommer2014', 'Mitarbeiter'),
 ('jens',   'jens123',    'Techniker');

INSERT INTO tblKunde (Name, Strasse, PLZ, Ort, Telefon, Email, Bemerkung, Aktiv) VALUES
 ('Bäckerei Hofmann',          'Lindenweg 4',        '79098', 'Freiburg',   '0761 123450', 'info@baeckerei-hofmann.example',  'Zugang über Hof', 1),
 ('Hausverwaltung Kessler',    'Am Markt 17',        '79100', 'Freiburg',   '0761 998877', 'kontakt@kessler-hv.example',      '12 Objekte, Rahmenvertrag', 1),
 ('Familie Brandt',            'Rosenstraße 9',      '79211', 'Denzlingen', '07666 4411',  NULL,                              NULL, 1),
 ('Gasthaus Zum Adler',        'Hauptstraße 52',     '79183', 'Waldkirch',  '07681 2020',  'adler@gasthaus.example',          'Nur vormittags', 1),
 ('Dr. Petra Wagner',          'Schlossbergring 3',  '79098', 'Freiburg',   '0761 556677', 'praxis-wagner@example.org',       'Praxis, Termin vorher absprechen', 1),
 ('Autohaus Rieger (alt)',     'Industriestraße 8',  '79108', 'Freiburg',   NULL,          NULL,                              'Firma aufgelöst', 0);

INSERT INTO tblAuftrag (KdNr, Datum, Status, Beschreibung, Techniker) VALUES
 (10001, '2014-03-12', 'Abgerechnet', 'Wartung Gastherme Backstube', 'Jens'),
 (10002, '2014-04-02', 'Erledigt',    'Heizkörper tauschen, Objekt Am Markt 17, 2. OG', 'Jens'),
 (10003, '2014-04-15', 'In Arbeit',   'Badsanierung: neue Armaturen und WC', 'Tobias'),
 (10004, '2014-04-20', 'Offen',       'Leck an Spülmaschinenanschluss Küche', NULL),
 (10005, '2014-04-22', 'Offen',       'Wartung Klimagerät Praxis', NULL),
 (10002, '2014-04-23', 'offen',       'Warmwasser fällt aus, Objekt Mühlenweg', NULL);

INSERT INTO tblPosition (AuftragsNr, Bezeichnung, Menge, Einheit, Einzelpreis) VALUES
 (2014001, 'Wartungspauschale Gastherme', 1,    'Stk', 149.00),
 (2014001, 'Arbeitszeit Monteur',         1.5,  'Std', 58.50),
 (2014001, 'Dichtungssatz',               1,    'Stk', 12.90),
 (2014002, 'Heizkörper Typ 22, 600x1000', 2,    'Stk', 189.00),
 (2014002, 'Thermostatventil',            2,    'Stk', 24.95),
 (2014002, 'Arbeitszeit Monteur',         3,    'Std', 58.50),
 (2014003, 'Einhebelmischer Waschtisch',  1,    'Stk', 119.00),
 (2014003, 'Wand-WC inkl. Spülkasten',    1,    'Stk', 349.00),
 (2014003, 'Arbeitszeit Monteur',         6.25, 'Std', 58.50);

INSERT INTO tblRechnung (AuftragsNr, Datum, Netto, MwSt, Brutto, Bezahlt) VALUES
 (2014001, '2014-03-14', 249.65, 47.43, 297.08, 1);

UPDATE tblAuftrag SET Status = 'Abgerechnet' WHERE AuftragsNr = 2014001;
GO
