using Auftragsverwaltung.Domain.Anmeldung;
using Auftragsverwaltung.Sicherheit;

namespace Auftragsverwaltung.Api.Sicherheit;

internal static class SicherheitExtensions
{
    public static IServiceCollection AddSicherheit(this IServiceCollection services, IConfiguration konfiguration, IHostEnvironment umgebung)
    {
        services.AddGemeinsameAnmeldung(konfiguration, umgebung.ContentRootPath, anmeldepflichtFuerAlles: true);
        services.AddSingleton<IPasswortHasher, PasswortHasher>();
        services.AddScoped<AnmeldungsVerwaltung>();
        return services;
    }
}
