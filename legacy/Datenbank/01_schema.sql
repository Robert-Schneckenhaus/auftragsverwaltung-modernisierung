-- Auftragsverwaltung Müller Haustechnik GmbH (fiktiv)
-- Schema Stand 2014, seitdem "organisch gewachsen"

IF DB_ID('Auftragsverwaltung') IS NULL
    CREATE DATABASE Auftragsverwaltung;
GO

USE Auftragsverwaltung;
GO

CREATE TABLE tblKunde (
    KdNr        INT IDENTITY(10001,1) PRIMARY KEY,
    Name        NVARCHAR(200) NOT NULL,
    Strasse     NVARCHAR(200) NULL,
    PLZ         NVARCHAR(10)  NULL,
    Ort         NVARCHAR(100) NULL,
    Telefon     NVARCHAR(50)  NULL,
    Email       NVARCHAR(200) NULL,
    Bemerkung   NVARCHAR(MAX) NULL,
    Aktiv       BIT NOT NULL DEFAULT 1,
    AngelegtAm  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE tblAuftrag (
    AuftragsNr   INT IDENTITY(2014001,1) PRIMARY KEY,
    KdNr         INT NOT NULL REFERENCES tblKunde(KdNr),
    Datum        DATETIME NOT NULL DEFAULT GETDATE(),
    -- Freitext-Status: 'Offen', 'In Arbeit', 'Erledigt', 'Abgerechnet'
    Status       NVARCHAR(50) NOT NULL DEFAULT 'Offen',
    Beschreibung NVARCHAR(MAX) NULL,
    Techniker    NVARCHAR(100) NULL
);

CREATE TABLE tblPosition (
    PosId        INT IDENTITY(1,1) PRIMARY KEY,
    AuftragsNr   INT NOT NULL REFERENCES tblAuftrag(AuftragsNr),
    Bezeichnung  NVARCHAR(300) NOT NULL,
    Menge        DECIMAL(10,2) NOT NULL,
    Einheit      NVARCHAR(20) NULL,
    Einzelpreis  DECIMAL(10,2) NOT NULL
);

CREATE TABLE tblRechnung (
    RechNr      INT IDENTITY(500001,1) PRIMARY KEY,
    AuftragsNr  INT NOT NULL REFERENCES tblAuftrag(AuftragsNr),
    Datum       DATETIME NOT NULL DEFAULT GETDATE(),
    Netto       DECIMAL(10,2) NOT NULL,
    MwSt        DECIMAL(10,2) NOT NULL,
    Brutto      DECIMAL(10,2) NOT NULL,
    Bezahlt     BIT NOT NULL DEFAULT 0
);

CREATE TABLE tblBenutzer (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Login     NVARCHAR(50)  NOT NULL UNIQUE,
    Passwort  NVARCHAR(100) NOT NULL, -- TODO: irgendwann hashen
    Rolle     NVARCHAR(20)  NOT NULL DEFAULT 'Mitarbeiter'
);
GO
