namespace Auftragsverwaltung.Domain.Kunden;

public sealed record KundeMitAuftraegen(Kunde Kunde, IReadOnlyList<AuftragKurz> Auftraege);

/// <summary>Anwendungsfälle rund um Kunden.</summary>
public sealed class KundenVerwaltung(IKundenRepository repository, TimeProvider zeit)
{
    public Task<IReadOnlyList<Kunde>> SuchenAsync(string? suche, bool nurAktive, CancellationToken ct = default)
        => repository.SuchenAsync(suche, nurAktive, ct);

    public async Task<Ergebnis<KundeMitAuftraegen>> LadenAsync(int kdNr, CancellationToken ct = default)
    {
        var kunde = await repository.LadenAsync(kdNr, ct);
        if (kunde is null)
            return Ergebnis.NichtGefunden<KundeMitAuftraegen>();

        var auftraege = await repository.AuftraegeVonAsync(kdNr, ct);
        return new KundeMitAuftraegen(kunde, auftraege);
    }

    public async Task<Ergebnis<Kunde>> AnlegenAsync(KundenDaten daten, CancellationToken ct = default)
    {
        var ergebnis = Kunde.Anlegen(daten, zeit.GetLocalNow().DateTime);
        if (ergebnis is Ergebnis<Kunde>.Erfolg { Wert: var kunde })
        {
            repository.Hinzufuegen(kunde);
            await repository.SpeichernAsync(ct);
        }
        return ergebnis;
    }

    public async Task<Ergebnis<Kunde>> AendernAsync(int kdNr, KundenDaten daten, CancellationToken ct = default)
    {
        var kunde = await repository.LadenAsync(kdNr, ct);
        if (kunde is null)
            return Ergebnis.NichtGefunden<Kunde>();

        var ergebnis = kunde.Aendern(daten);
        if (ergebnis is Ergebnis<Kunde>.Erfolg)
            await repository.SpeichernAsync(ct);
        return ergebnis;
    }

    public async Task<Ergebnis<Erledigt>> DeaktivierenAsync(int kdNr, CancellationToken ct = default)
    {
        var kunde = await repository.LadenAsync(kdNr, ct);
        if (kunde is null)
            return Ergebnis.NichtGefunden<Erledigt>();

        var offene = await repository.ZaehleOffeneAuftraegeAsync(kdNr, ct);
        var ergebnis = kunde.Deaktivieren(offene);
        if (ergebnis is Ergebnis<Erledigt>.Erfolg)
            await repository.SpeichernAsync(ct);
        return ergebnis;
    }
}
