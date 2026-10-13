using Auftragsverwaltung.Sicherheit;

// Strangler-Fig-Proxy (ADR 0001): Bereits migrierte Routen gehen an das moderne Backend,
// alles andere (inkl. AngularJS-Frontend) weiterhin an das Legacy-System.
// Welche Routen umgeschaltet sind und welche Anmeldung sie verlangen, steht in appsettings.json.
//
// Der Proxy prüft das Anmelde-Cookie der modernen API selbst (ADR 0003). Dadurch sind auch
// Legacy-Endpunkte geschützt, die selbst keine Anmeldung kennen.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGemeinsameAnmeldung(builder.Configuration, builder.Environment.ContentRootPath, anmeldepflichtFuerAlles: false);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
