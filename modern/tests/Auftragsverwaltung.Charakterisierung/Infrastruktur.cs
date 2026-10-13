using System.Net.Http.Json;
using System.Text.Json;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Auftragsverwaltung.Charakterisierung;

/// <summary>
/// Läuft nur, wenn AV_BASE_URL gesetzt ist, z. B.
/// http://localhost:51400/ (Legacy, IIS Express) oder http://localhost:5080/ (modern).
/// Voraussetzung: Datenbank frisch aus legacy/Datenbank/*.sql angelegt.
/// </summary>
public sealed class CharakterisierungFactAttribute : FactAttribute
{
    public CharakterisierungFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AV_BASE_URL")))
            Skip = "AV_BASE_URL nicht gesetzt";
    }
}

public sealed class ApiFixture : IDisposable
{
    public HttpClient Client { get; }

    public ApiFixture()
    {
        var basis = Environment.GetEnvironmentVariable("AV_BASE_URL") ?? "http://localhost/";
        if (!basis.EndsWith('/')) basis += "/";
        Client = new HttpClient { BaseAddress = new Uri(basis) };
    }

    public void Dispose() => Client.Dispose();
}

internal static class JsonHilfen
{
    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage antwort)
        => await antwort.Content.ReadFromJsonAsync<JsonElement>();

    public static string? Text(this JsonElement e, string name) => e.GetProperty(name).GetString();

    public static IEnumerable<string> Eigenschaften(this JsonElement e)
        => e.EnumerateObject().Select(p => p.Name);

    /// <summary>Fehlermeldung im Web-API-2-Format {"Message": "..."}.</summary>
    public static async Task<string?> FehlermeldungAsync(this HttpResponseMessage antwort)
        => (await antwort.JsonAsync()).Text("Message");
}
