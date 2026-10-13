namespace Auftragsverwaltung.Domain.Kunden;

/// <summary>Kurzansicht eines Auftrags in der Kundendetailseite.</summary>
public sealed record AuftragKurz(int AuftragsNr, DateTime Datum, string Status, string? Beschreibung);

public interface IKundenRepository
{
    /// <summary>
    /// Sucht in Name und Ort (enthält) sowie PLZ (beginnt mit), sortiert nach Name.
    /// Groß-/Kleinschreibung richtet sich wie im Legacy-System nach der Datenbank-Collation.
    /// </summary>
    Task<IReadOnlyList<Kunde>> SuchenAsync(string? suche, bool nurAktive, CancellationToken ct);

    Task<Kunde?> LadenAsync(int kdNr, CancellationToken ct);

    /// <summary>Aufträge des Kunden, neueste zuerst.</summary>
    Task<IReadOnlyList<AuftragKurz>> AuftraegeVonAsync(int kdNr, CancellationToken ct);

    /// <summary>
    /// Aufträge, deren Status weder "Abgerechnet" noch "Erledigt" ist.
    /// Der Vergleich findet bewusst in der Datenbank statt: Wie im Legacy-System
    /// zählt z. B. "erledigt" (klein) wegen der Collation als erledigt.
    /// </summary>
    Task<int> ZaehleOffeneAuftraegeAsync(int kdNr, CancellationToken ct);

    void Hinzufuegen(Kunde kunde);

    Task SpeichernAsync(CancellationToken ct);
}
