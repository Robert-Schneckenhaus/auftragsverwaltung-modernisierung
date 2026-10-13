using Auftragsverwaltung.Api.Sicherheit;
using Auftragsverwaltung.Domain.Kunden;
using Auftragsverwaltung.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AvDb")
    ?? throw new InvalidOperationException(
        "Connection-String 'AvDb' fehlt. Setzen über appsettings.Development.json oder Umgebungsvariable ConnectionStrings__AvDb.");

builder.Services.AddInfrastruktur(connectionString);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<KundenVerwaltung>();
builder.Services.AddSicherheit(builder.Configuration, builder.Environment);

builder.Services
    .AddControllers()
    // Das AngularJS-Frontend erwartet PascalCase wie bei Web API 2 (Newtonsoft-Standard)
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok")).AllowAnonymous();

app.Run();

public partial class Program;
