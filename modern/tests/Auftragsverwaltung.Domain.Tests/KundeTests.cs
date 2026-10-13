using Auftragsverwaltung.Domain;
using Auftragsverwaltung.Domain.Kunden;

namespace Auftragsverwaltung.Domain.Tests;

public class KundeTests
{
    private static readonly DateTime Jetzt = new(2026, 10, 13, 9, 30, 0);

    private static KundenDaten Daten(string? name = "Bäckerei Hofmann", string? plz = "79098")
        => new(name, "Lindenweg 4", plz, "Freiburg", "0761 123450", "info@example.org", null);

    [Fact]
    public void Anlegen_setzt_Aktiv_und_Anlagedatum()
    {
        var kunde = Assert.IsType<Ergebnis<Kunde>.Erfolg>(Kunde.Anlegen(Daten(), Jetzt)).Wert;

        Assert.True(kunde.Aktiv);
        Assert.Equal(Jetzt, kunde.AngelegtAm);
        Assert.Equal("Bäckerei Hofmann", kunde.Name);
        Assert.Equal("79098", kunde.Plz);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Anlegen_ohne_Name_wird_abgelehnt(string? name)
    {
        var fehler = Assert.IsType<Ergebnis<Kunde>.Fehler>(Kunde.Anlegen(Daten(name: name), Jetzt));
        Assert.Equal("Name fehlt", fehler.Meldung);
    }

    [Fact]
    public void Name_wird_vor_PLZ_geprueft()
    {
        var fehler = Assert.IsType<Ergebnis<Kunde>.Fehler>(Kunde.Anlegen(Daten(name: null, plz: "1"), Jetzt));
        Assert.Equal("Name fehlt", fehler.Meldung);
    }

    [Theory]
    [InlineData("8001")]   // Schweizer PLZ, siehe Altlasten-Katalog
    [InlineData("790988")]
    [InlineData("")]       // Legacy-Eigenheit: leere PLZ wird abgelehnt, null nicht
    public void PLZ_mit_falscher_Laenge_wird_abgelehnt(string plz)
    {
        var fehler = Assert.IsType<Ergebnis<Kunde>.Fehler>(Kunde.Anlegen(Daten(plz: plz), Jetzt));
        Assert.Equal("PLZ muss 5-stellig sein", fehler.Meldung);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ABCDE")]  // Legacy-Eigenheit: nur die Länge wird geprüft
    public void PLZ_wird_nur_auf_Laenge_geprueft(string? plz)
        => Assert.IsType<Ergebnis<Kunde>.Erfolg>(Kunde.Anlegen(Daten(plz: plz), Jetzt));

    [Fact]
    public void Aendern_uebernimmt_Daten_aber_nicht_Status_und_Anlagedatum()
    {
        var kunde = ((Ergebnis<Kunde>.Erfolg)Kunde.Anlegen(Daten(), Jetzt)).Wert;
        kunde.Deaktivieren(0);

        var ergebnis = kunde.Aendern(Daten(name: "Bäckerei Hofmann & Sohn", plz: "79100"));

        Assert.IsType<Ergebnis<Kunde>.Erfolg>(ergebnis);
        Assert.Equal("Bäckerei Hofmann & Sohn", kunde.Name);
        Assert.Equal("79100", kunde.Plz);
        Assert.False(kunde.Aktiv);
        Assert.Equal(Jetzt, kunde.AngelegtAm);
    }

    [Fact]
    public void Aendern_mit_ungueltigen_Daten_laesst_Kunde_unveraendert()
    {
        var kunde = ((Ergebnis<Kunde>.Erfolg)Kunde.Anlegen(Daten(), Jetzt)).Wert;

        var fehler = Assert.IsType<Ergebnis<Kunde>.Fehler>(kunde.Aendern(Daten(plz: "123")));

        Assert.Equal("PLZ muss 5-stellig sein", fehler.Meldung);
        Assert.Equal("79098", kunde.Plz);
    }

    [Fact]
    public void Deaktivieren_mit_offenen_Auftraegen_wird_abgelehnt()
    {
        var kunde = ((Ergebnis<Kunde>.Erfolg)Kunde.Anlegen(Daten(), Jetzt)).Wert;

        var fehler = Assert.IsType<Ergebnis<Erledigt>.Fehler>(kunde.Deaktivieren(2));

        Assert.Equal("Kunde hat noch 2 offene Aufträge", fehler.Meldung);
        Assert.True(kunde.Aktiv);
    }

    [Fact]
    public void Deaktivieren_ohne_offene_Auftraege()
    {
        var kunde = ((Ergebnis<Kunde>.Erfolg)Kunde.Anlegen(Daten(), Jetzt)).Wert;

        Assert.IsType<Ergebnis<Erledigt>.Erfolg>(kunde.Deaktivieren(0));
        Assert.False(kunde.Aktiv);
    }
}
