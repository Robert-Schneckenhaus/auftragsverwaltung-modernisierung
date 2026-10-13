using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Auftragsverwaltung.Charakterisierung;

/// <summary>
/// Hält fest, wie sich api/login im Legacy-System verhält.
/// Bewusst nicht festgehalten: Im Legacy-System wird das Passwort in SQL ohne
/// Beachtung der Groß-/Kleinschreibung verglichen ("ADMIN" funktioniert). Das ist
/// eine Sicherheitslücke und wird laut ADR 0003 nicht übernommen.
/// </summary>
public class LoginTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly JsonSerializerOptions WieFrontend = new() { PropertyNamingPolicy = null };

    private Task<HttpResponseMessage> LoginAsync(object daten)
        => api.Client.PostAsync("api/login", JsonContent.Create(daten, options: WieFrontend));

    [CharakterisierungFact]
    public async Task Erfolgreicher_Login_liefert_Login_Rolle_und_Token()
    {
        var antwort = await LoginAsync(new { Login = "admin", Passwort = "admin" });

        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        var json = await antwort.JsonAsync();
        Assert.Equal(new[] { "Login", "Rolle", "Token" }, json.Eigenschaften());
        Assert.Equal("admin", json.Text("Login"));
        Assert.Equal("Admin", json.Text("Rolle"));

        // Das Frontend speichert das Token nur, der Inhalt ist Base64 aus "Login:Rolle"
        Assert.Equal("admin:Admin", Encoding.UTF8.GetString(Convert.FromBase64String(json.Text("Token")!)));
    }

    [CharakterisierungFact]
    public async Task Login_funktioniert_wiederholt()
    {
        // Wichtig für die Hash-Migration: auch der zweite Login muss klappen
        for (var i = 0; i < 2; i++)
        {
            var antwort = await LoginAsync(new { Login = "sabine", Passwort = "sommer2014" });
            Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
            Assert.Equal("Mitarbeiter", (await antwort.JsonAsync()).Text("Rolle"));
        }
    }

    [CharakterisierungFact]
    public async Task Falsches_Passwort_ergibt_400()
    {
        var antwort = await LoginAsync(new { Login = "admin", Passwort = "falsch" });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Login oder Passwort falsch", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Unbekannter_Benutzer_ergibt_dieselbe_Meldung()
    {
        var antwort = await LoginAsync(new { Login = "gibtesnicht", Passwort = "admin" });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Login oder Passwort falsch", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Fehlende_Felder_ergeben_Login_oder_Passwort_falsch()
    {
        var antwort = await LoginAsync(new { });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Login oder Passwort falsch", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Leerer_Body_ergibt_Keine_Daten()
    {
        var antwort = await api.Client.PostAsync("api/login",
            new StringContent("", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Keine Daten", await antwort.FehlermeldungAsync());
    }
}
