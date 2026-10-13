using System.Security.Claims;
using System.Text;
using Auftragsverwaltung.Domain;
using Auftragsverwaltung.Domain.Anmeldung;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Auftragsverwaltung.Api.Controllers;

[Route("api")]
[AllowAnonymous]
public sealed class AnmeldungController(AnmeldungsVerwaltung anmeldung) : ControllerBase
{
    /// <summary>
    /// Antwort wie im Legacy-System, zusätzlich wird ein HttpOnly-Cookie gesetzt (ADR 0003).
    /// Das Token bleibt nur für die Anzeige im AngularJS-Frontend erhalten und wird nicht geprüft.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Anmelden(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] AnmeldeDaten? daten, CancellationToken ct)
    {
        var ergebnis = await anmeldung.AnmeldenAsync(daten, ct);
        if (ergebnis is not Ergebnis<Benutzer>.Erfolg { Wert: var benutzer })
            return ergebnis.AlsAntwort(_ => Ok());

        var identitaet = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, benutzer.Id.ToString()),
                new Claim(ClaimTypes.Name, benutzer.Login),
                new Claim(ClaimTypes.Role, benutzer.Rolle)
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identitaet),
            new AuthenticationProperties { IsPersistent = true });

        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{benutzer.Login}:{benutzer.Rolle}"));
        return Ok(new LoginAntwort(benutzer.Login, benutzer.Rolle, token));
    }

    /// <summary>Neu gegenüber dem Legacy-System: beendet die Sitzung serverseitig.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Abmelden()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}

public sealed record LoginAntwort(string Login, string Rolle, string Token);
