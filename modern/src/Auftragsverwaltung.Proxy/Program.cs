// Strangler-Fig-Proxy (ADR 0001): Bereits migrierte Routen gehen an das moderne Backend,
// alles andere (inkl. AngularJS-Frontend) weiterhin an das Legacy-System.
// Welche Routen umgeschaltet sind, steht ausschließlich in appsettings.json.

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();
