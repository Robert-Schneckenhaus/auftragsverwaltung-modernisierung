using Auftragsverwaltung.Domain;
using Auftragsverwaltung.Domain.Kunden;

namespace Auftragsverwaltung.Domain.Tests;

public class KundenVerwaltungTests
{
    private readonly FakeRepository repo = new();
    private readonly KundenVerwaltung verwaltung;

    public KundenVerwaltungTests()
        => verwaltung = new KundenVerwaltung(repo, TimeProvider.System);

    private static KundenDaten Daten(string? name = "Gasthaus Zum Adler")
        => new(name, null, null, "Waldkirch", null, null, null);

    [Fact]
    public async Task Anlegen_speichert_nur_gueltige_Kunden()
    {
        await verwaltung.AnlegenAsync(Daten(name: ""));
        Assert.Empty(repo.Kunden);
        Assert.Equal(0, repo.Speichervorgaenge);

        await verwaltung.AnlegenAsync(Daten());
        Assert.Single(repo.Kunden);
        Assert.Equal(1, repo.Speichervorgaenge);
    }

    [Fact]
    public async Task Unbekannter_Kunde_ergibt_NichtGefunden()
    {
        Assert.IsType<Ergebnis<KundeMitAuftraegen>.NichtGefunden>(await verwaltung.LadenAsync(4711));
        Assert.IsType<Ergebnis<Kunde>.NichtGefunden>(await verwaltung.AendernAsync(4711, Daten()));
        Assert.IsType<Ergebnis<Erledigt>.NichtGefunden>(await verwaltung.DeaktivierenAsync(4711));
    }

    [Fact]
    public async Task Deaktivieren_beruecksichtigt_offene_Auftraege()
    {
        var kunde = ((Ergebnis<Kunde>.Erfolg)await verwaltung.AnlegenAsync(Daten())).Wert;
        repo.OffeneAuftraege = 1;

        var fehler = Assert.IsType<Ergebnis<Erledigt>.Fehler>(await verwaltung.DeaktivierenAsync(kunde.KdNr));

        Assert.Equal("Kunde hat noch 1 offene Aufträge", fehler.Meldung);
        Assert.True(kunde.Aktiv);
    }

    private sealed class FakeRepository : IKundenRepository
    {
        public List<Kunde> Kunden { get; } = [];
        public int Speichervorgaenge { get; private set; }
        public int OffeneAuftraege { get; set; }

        public Task<IReadOnlyList<Kunde>> SuchenAsync(string? suche, bool nurAktive, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<Kunde>>(Kunden);

        public Task<Kunde?> LadenAsync(int kdNr, CancellationToken ct)
            => Task.FromResult(Kunden.FirstOrDefault(k => k.KdNr == kdNr));

        public Task<IReadOnlyList<AuftragKurz>> AuftraegeVonAsync(int kdNr, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<AuftragKurz>>([]);

        public Task<int> ZaehleOffeneAuftraegeAsync(int kdNr, CancellationToken ct)
            => Task.FromResult(OffeneAuftraege);

        public void Hinzufuegen(Kunde kunde) => Kunden.Add(kunde);

        public Task SpeichernAsync(CancellationToken ct)
        {
            Speichervorgaenge++;
            return Task.CompletedTask;
        }
    }
}
