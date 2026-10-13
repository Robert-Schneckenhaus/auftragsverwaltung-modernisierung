using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auftragsverwaltung.Sicherheit;

public sealed class SicherheitOptionen
{
    public const string Abschnitt = "Sicherheit";

    /// <summary>
    /// Feature-Flag aus ADR 0003. Aus: Verhalten wie im Legacy-System (keine Anmeldung nötig).
    /// An: API und Proxy verlangen eine Anmeldung, Rechnungen nur für Admins.
    /// </summary>
    public bool AnmeldungErforderlich { get; set; }

    /// <summary>
    /// Verzeichnis für die Data-Protection-Schlüssel. API und Proxy müssen dasselbe Verzeichnis
    /// verwenden, damit der Proxy das von der API ausgestellte Cookie prüfen kann.
    /// In Produktion zusätzlich verschlüsseln (z. B. ProtectKeysWithCertificate oder DPAPI).
    /// </summary>
    public string? SchluesselPfad { get; set; }
}

public static class Richtlinien
{
    public const string Angemeldet = nameof(Angemeldet);
    public const string NurAdmin = nameof(NurAdmin);
}

public static class GemeinsameAnmeldung
{
    public const string CookieName = "av.anmeldung";
    public const string AdminRolle = "Admin";
    private const string AnwendungsName = "Auftragsverwaltung";

    /// <param name="inhaltsVerzeichnis">Basis für einen relativen <see cref="SicherheitOptionen.SchluesselPfad"/>.</param>
    /// <param name="anmeldepflichtFuerAlles">
    /// true für die API (jeder Endpunkt ohne [AllowAnonymous] ist geschützt),
    /// false für den Proxy (Schutz pro Route über "AuthorizationPolicy").
    /// </param>
    public static IServiceCollection AddGemeinsameAnmeldung(
        this IServiceCollection services, IConfiguration konfiguration, string inhaltsVerzeichnis, bool anmeldepflichtFuerAlles)
    {
        var optionen = konfiguration.GetSection(SicherheitOptionen.Abschnitt).Get<SicherheitOptionen>() ?? new();

        var dataProtection = services.AddDataProtection().SetApplicationName(AnwendungsName);
        if (!string.IsNullOrWhiteSpace(optionen.SchluesselPfad))
            dataProtection.PersistKeysToFileSystem(
                new DirectoryInfo(Path.GetFullPath(optionen.SchluesselPfad, inhaltsVerzeichnis)));

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o =>
            {
                o.Cookie.Name = CookieName;
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Strict;                   // Schutz gegen CSRF
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // lokal http, produktiv https
                o.ExpireTimeSpan = TimeSpan.FromHours(8);
                o.SlidingExpiration = true;

                // API statt Weiterleitung auf eine Login-Seite
                o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
            });

        services.AddAuthorization(o =>
        {
            if (optionen.AnmeldungErforderlich)
            {
                o.AddPolicy(Richtlinien.Angemeldet, p => p.RequireAuthenticatedUser());
                o.AddPolicy(Richtlinien.NurAdmin, p => p.RequireAuthenticatedUser().RequireRole(AdminRolle));
                if (anmeldepflichtFuerAlles)
                    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            }
            else
            {
                // Flag aus: Richtlinien existieren, lassen aber alles durch
                o.AddPolicy(Richtlinien.Angemeldet, p => p.RequireAssertion(_ => true));
                o.AddPolicy(Richtlinien.NurAdmin, p => p.RequireAssertion(_ => true));
            }
        });

        return services;
    }
}
