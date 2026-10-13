using Auftragsverwaltung.Domain.Kunden;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Auftragsverwaltung.Api.Controllers;

/// <summary>
/// Gleicher API-Vertrag wie legacy/…/KundenController.cs, abgesichert durch die Charakterisierungstests.
/// Bewusst ohne [ApiController], damit Fehlerantworten nicht als ProblemDetails, sondern im Legacy-Format kommen.
/// </summary>
[Route("api/kunden")]
public sealed class KundenController(KundenVerwaltung kunden) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Suchen(string? suche, bool nurAktive = true, CancellationToken ct = default)
    {
        var liste = await kunden.SuchenAsync(suche, nurAktive, ct);
        return Ok(liste.Select(KundeDto.Von));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Laden(int id, CancellationToken ct)
        => (await kunden.LadenAsync(id, ct))
            .AlsAntwort(k => Ok(KundeDto.Von(k.Kunde, k.Auftraege)));

    [HttpPost("")]
    public async Task<IActionResult> Anlegen(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] KundeEingabe? eingabe, CancellationToken ct)
        => (await kunden.AnlegenAsync(KundeEingabe.AlsDaten(eingabe), ct))
            .AlsAntwort(k => Ok(KundeDto.Von(k)));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Aendern(
        int id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] KundeEingabe? eingabe, CancellationToken ct)
        => (await kunden.AendernAsync(id, KundeEingabe.AlsDaten(eingabe), ct))
            .AlsAntwort(k => Ok(KundeDto.Von(k)));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deaktivieren(int id, CancellationToken ct)
        => (await kunden.DeaktivierenAsync(id, ct))
            .AlsAntwort(_ => Ok());
}

public sealed record KundeEingabe(
    string? Name, string? Strasse, string? PLZ, string? Ort, string? Telefon, string? Email, string? Bemerkung)
{
    public static KundenDaten AlsDaten(KundeEingabe? e)
        => new(e?.Name, e?.Strasse, e?.PLZ, e?.Ort, e?.Telefon, e?.Email, e?.Bemerkung);
}

/// <summary>
/// Antwortformat wie im Legacy-System. <c>Auftraege</c> ist dort in Listen und nach dem Speichern
/// immer <c>null</c> (serialisierte EF6-Entität) und nur in der Detailansicht gefüllt.
/// Das Feld bleibt erhalten, bis das Frontend migriert ist.
/// </summary>
public sealed record KundeDto(
    int KdNr,
    string Name,
    string? Strasse,
    string? PLZ,
    string? Ort,
    string? Telefon,
    string? Email,
    string? Bemerkung,
    bool Aktiv,
    DateTime AngelegtAm,
    IReadOnlyList<AuftragKurz>? Auftraege)
{
    public static KundeDto Von(Kunde k) => Von(k, null);

    public static KundeDto Von(Kunde k, IReadOnlyList<AuftragKurz>? auftraege)
        => new(k.KdNr, k.Name, k.Strasse, k.Plz, k.Ort, k.Telefon, k.Email, k.Bemerkung, k.Aktiv, k.AngelegtAm, auftraege);
}
