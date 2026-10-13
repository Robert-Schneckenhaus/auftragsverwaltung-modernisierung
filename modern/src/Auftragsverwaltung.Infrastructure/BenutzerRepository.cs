using Auftragsverwaltung.Domain.Anmeldung;
using Microsoft.EntityFrameworkCore;

namespace Auftragsverwaltung.Infrastructure;

internal sealed class BenutzerRepository(AvDbContext db) : IBenutzerRepository
{
    public Task<Benutzer?> LadenAsync(string login, CancellationToken ct)
        => db.Benutzer.FirstOrDefaultAsync(b => b.Login == login, ct);

    public Task SpeichernAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
