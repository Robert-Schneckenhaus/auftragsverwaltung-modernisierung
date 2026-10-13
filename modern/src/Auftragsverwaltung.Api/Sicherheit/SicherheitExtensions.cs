using Auftragsverwaltung.Domain.Anmeldung;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Auftragsverwaltung.Api.Sicherheit;

public sealed class SicherheitOptionen
{
    public const string Abschnitt = "Sicherheit";

    /// <summary>
    /// Feature-Flag aus ADR 0003. Aus: API-Vertrag wie im Legacy-System (ohne Anmeldung).
    /// An: Alle Endpunkte außer Login und Health erfordern eine Anmeldung.
    /// </summary>
    public bool AnmeldungErforderlich { get; set; }
}

public static class Richtlinien
{
    public const string NurAdmin = nameof(NurAdmin);
}

internal static class SicherheitExtensions
{
    public static IServiceCollection AddSicherheit(this IServiceCollection services, IConfiguration konfiguration)
    {
        var optionen = konfiguration.GetSection(SicherheitOptionen.Abschnitt).Get<SicherheitOptionen>() ?? new();

        services.AddSingleton<IPasswortHasher, PasswortHasher>();
        services.AddScoped<AnmeldungsVerwaltung>();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o =>
            {
                o.Cookie.Name = "av.anmeldung";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Strict;   // Schutz gegen CSRF
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // lokal http, produktiv https
                o.ExpireTimeSpan = TimeSpan.FromHours(8);
                o.SlidingExpiration = true;

                // API statt Weiterleitung auf eine Login-Seite
                o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
            });

        services.AddAuthorization(o =>
        {
            o.AddPolicy(Richtlinien.NurAdmin, p => p.RequireRole(Rollen.Admin));

            if (optionen.AnmeldungErforderlich)
                o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });

        return services;
    }
}
