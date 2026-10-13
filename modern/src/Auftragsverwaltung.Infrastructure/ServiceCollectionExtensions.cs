using Auftragsverwaltung.Domain.Anmeldung;
using Auftragsverwaltung.Domain.Kunden;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auftragsverwaltung.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastruktur(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AvDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<IKundenRepository, KundenRepository>();
        services.AddScoped<IBenutzerRepository, BenutzerRepository>();
        return services;
    }
}
