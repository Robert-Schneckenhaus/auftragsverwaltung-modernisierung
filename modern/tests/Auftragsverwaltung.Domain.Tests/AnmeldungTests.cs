using Auftragsverwaltung.Domain;
using Auftragsverwaltung.Domain.Anmeldung;

namespace Auftragsverwaltung.Domain.Tests;

public class AnmeldungTests
{
    /// <summary>Durchschaubarer Hasher, damit die Tests die Migration beobachten können.</summary>
    private sealed class TestHasher : IPasswortHasher
    {
        public bool AltesVerfahren { get; set; }
        public string Hashen(string passwort) => $"hash:{passwort}";
        public PasswortPruefung Pruefen(string hash, string passwort)
            => hash != $"hash:{passwort}" ? PasswortPruefung.Falsch
             : AltesVerfahren ? PasswortPruefung.RichtigNeuHashen
             : PasswortPruefung.Richtig;
    }

    private sealed class FakeRepository(params Benutzer[] benutzer) : IBenutzerRepository
    {
        public int Speichervorgaenge { get; private set; }

        public Task<Benutzer?> LadenAsync(string login, CancellationToken ct)
            => Task.FromResult(benutzer.FirstOrDefault(b => string.Equals(b.Login, login, StringComparison.OrdinalIgnoreCase)));

        public Task SpeichernAsync(CancellationToken ct)
        {
            Speichervorgaenge++;
            return Task.CompletedTask;
        }
    }

    private readonly TestHasher hasher = new();

    [Fact]
    public void Erster_Login_ersetzt_Klartext_durch_Hash()
    {
        var benutzer = Benutzer.AusLegacy("admin", "admin", Rollen.Admin);

        Assert.True(benutzer.PasswortPruefen("admin", hasher));

        Assert.Equal("hash:admin", benutzer.PasswortHash);
        Assert.Equal("", benutzer.Passwort);
        Assert.False(benutzer.KlartextVorhanden);
    }

    [Fact]
    public void Nach_der_Migration_wird_gegen_den_Hash_geprueft()
    {
        var benutzer = Benutzer.AusLegacy("admin", "admin", Rollen.Admin);
        benutzer.PasswortPruefen("admin", hasher);

        Assert.True(benutzer.PasswortPruefen("admin", hasher));
        Assert.False(benutzer.PasswortPruefen("falsch", hasher));
    }

    [Theory]
    [InlineData("falsch")]
    [InlineData("ADMIN")]  // Legacy akzeptierte das wegen der SQL-Collation
    [InlineData("")]
    public void Falsches_Passwort_laesst_Klartext_unveraendert(string passwort)
    {
        var benutzer = Benutzer.AusLegacy("admin", "admin", Rollen.Admin);

        Assert.False(benutzer.PasswortPruefen(passwort, hasher));

        Assert.Null(benutzer.PasswortHash);
        Assert.Equal("admin", benutzer.Passwort);
    }

    [Fact]
    public void Leeres_Klartextpasswort_erlaubt_keinen_Login()
        => Assert.False(Benutzer.AusLegacy("alt", "", "Mitarbeiter").PasswortPruefen("", hasher));

    [Fact]
    public void Veralteter_Hash_wird_beim_Login_erneuert()
    {
        var benutzer = Benutzer.AusLegacy("admin", "admin", Rollen.Admin);
        benutzer.PasswortPruefen("admin", hasher);
        hasher.AltesVerfahren = true;

        Assert.True(benutzer.PasswortPruefen("admin", hasher));
        Assert.Equal("hash:admin", benutzer.PasswortHash);
    }

    [Fact]
    public async Task Anmelden_speichert_den_neuen_Hash()
    {
        var repo = new FakeRepository(Benutzer.AusLegacy("sabine", "sommer2014", "Mitarbeiter"));
        var verwaltung = new AnmeldungsVerwaltung(repo, hasher);

        var benutzer = Assert.IsType<Ergebnis<Benutzer>.Erfolg>(
            await verwaltung.AnmeldenAsync(new AnmeldeDaten("Sabine", "sommer2014"))).Wert;

        Assert.Equal("sabine", benutzer.Login);
        Assert.Equal("hash:sommer2014", benutzer.PasswortHash);
        Assert.Equal(1, repo.Speichervorgaenge);
    }

    [Fact]
    public async Task Unbekannter_Benutzer_und_falsches_Passwort_sind_nicht_unterscheidbar()
    {
        var repo = new FakeRepository(Benutzer.AusLegacy("admin", "admin", Rollen.Admin));
        var verwaltung = new AnmeldungsVerwaltung(repo, hasher);

        var unbekannt = Assert.IsType<Ergebnis<Benutzer>.Fehler>(await verwaltung.AnmeldenAsync(new AnmeldeDaten("niemand", "admin")));
        var falsch = Assert.IsType<Ergebnis<Benutzer>.Fehler>(await verwaltung.AnmeldenAsync(new AnmeldeDaten("admin", "x")));
        var leer = Assert.IsType<Ergebnis<Benutzer>.Fehler>(await verwaltung.AnmeldenAsync(new AnmeldeDaten(null, null)));

        Assert.Equal(AnmeldungsVerwaltung.FalscheAnmeldung, unbekannt.Meldung);
        Assert.Equal(unbekannt.Meldung, falsch.Meldung);
        Assert.Equal(unbekannt.Meldung, leer.Meldung);
        Assert.Equal(0, repo.Speichervorgaenge);
    }

    [Fact]
    public async Task Ohne_Daten_kommt_Keine_Daten()
    {
        var verwaltung = new AnmeldungsVerwaltung(new FakeRepository(), hasher);

        var fehler = Assert.IsType<Ergebnis<Benutzer>.Fehler>(await verwaltung.AnmeldenAsync(null));

        Assert.Equal("Keine Daten", fehler.Meldung);
    }
}
