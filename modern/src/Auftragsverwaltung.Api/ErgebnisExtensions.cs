using Auftragsverwaltung.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Auftragsverwaltung.Api;

internal static class ErgebnisExtensions
{
    /// <summary>
    /// Übersetzt ein fachliches Ergebnis in die HTTP-Antwort, die das Legacy-System liefert:
    /// Fehler als 400 mit {"Message": "..."}, nicht gefunden als 404 ohne Inhalt.
    /// </summary>
    public static IActionResult AlsAntwort<T>(this Ergebnis<T> ergebnis, Func<T, IActionResult> beiErfolg)
        => ergebnis switch
        {
            Ergebnis<T>.Erfolg e => beiErfolg(e.Wert),
            Ergebnis<T>.NichtGefunden => new NotFoundResult(),
            Ergebnis<T>.Fehler f => new BadRequestObjectResult(new FehlerAntwort(f.Meldung)),
            _ => throw new InvalidOperationException("Unbekanntes Ergebnis")
        };
}

/// <summary>Fehlerformat von ASP.NET Web API 2, auf das sich das Frontend verlässt.</summary>
internal sealed record FehlerAntwort(string Message);
