using Auftragsverwaltung.Domain.Kunden;
using Microsoft.EntityFrameworkCore;

namespace Auftragsverwaltung.Infrastructure;

internal sealed class KundenRepository(AvDbContext db) : IKundenRepository
{
    public async Task<IReadOnlyList<Kunde>> SuchenAsync(string? suche, bool nurAktive, CancellationToken ct)
    {
        var query = db.Kunden.AsNoTracking();

        if (nurAktive)
            query = query.Where(k => k.Aktiv);

        if (!string.IsNullOrEmpty(suche))
            query = query.Where(k => k.Name.Contains(suche)
                                  || k.Ort!.Contains(suche)
                                  || k.Plz!.StartsWith(suche));

        return await query.OrderBy(k => k.Name).ToListAsync(ct);
    }

    public Task<Kunde?> LadenAsync(int kdNr, CancellationToken ct)
        => db.Kunden.FirstOrDefaultAsync(k => k.KdNr == kdNr, ct);

    public async Task<IReadOnlyList<AuftragKurz>> AuftraegeVonAsync(int kdNr, CancellationToken ct)
        => await db.Auftraege.AsNoTracking()
            .Where(a => a.KdNr == kdNr)
            .OrderByDescending(a => a.Datum)
            .Select(a => new AuftragKurz(a.AuftragsNr, a.Datum, a.Status, a.Beschreibung))
            .ToListAsync(ct);

    public Task<int> ZaehleOffeneAuftraegeAsync(int kdNr, CancellationToken ct)
        => db.Auftraege.CountAsync(a => a.KdNr == kdNr
                                     && a.Status != "Abgerechnet"
                                     && a.Status != "Erledigt", ct);

    public void Hinzufuegen(Kunde kunde) => db.Kunden.Add(kunde);

    public Task SpeichernAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
