using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Auftragsverwaltung.Sicherheitstests;

public sealed class SicherheitFactAttribute : FactAttribute
{
    public SicherheitFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AV_SICHERHEIT_URL")))
            Skip = "AV_SICHERHEIT_URL nicht gesetzt";
    }
}

/// <summary>
/// Sicherheits-Baseline aus ADR 0003. Diese Tests beschreiben bewusst Verhalten,
/// das vom Legacy-System abweicht, und laufen deshalb nur gegen das moderne Backend.
/// </summary>
public class SicherheitsTests
{
    private static readonly JsonSerializerOptions WieFrontend = new() { PropertyNamingPolicy = null };

    /// <summary>Jeder Test bekommt einen eigenen "Browser" mit eigenem Cookie-Speicher.</summary>
    private static HttpClient NeuerBrowser(bool cookieSpeicher = true)
    {
        var basis = Environment.GetEnvironmentVariable("AV_SICHERHEIT_URL")!;
        if (!basis.EndsWith('/')) basis += "/";
        return new HttpClient(new HttpClientHandler { UseCookies = cookieSpeicher, CookieContainer = new CookieContainer() })
        {
            BaseAddress = new Uri(basis)
        };
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient browser, string login, string passwort)
        => browser.PostAsync("api/login", JsonContent.Create(new { Login = login, Passwort = passwort }, options: WieFrontend));

    [SicherheitFact]
    public async Task Ohne_Anmeldung_ist_die_API_gesperrt()
    {
        using var browser = NeuerBrowser();

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden/10001")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.DeleteAsync("api/kunden/10001")).StatusCode);
    }

    [SicherheitFact]
    public async Task Health_ist_ohne_Anmeldung_erreichbar()
    {
        using var browser = NeuerBrowser();
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("health")).StatusCode);
    }

    [SicherheitFact]
    public async Task Login_setzt_ein_geschuetztes_Cookie_und_oeffnet_die_API()
    {
        using var browser = NeuerBrowser();

        var antwort = await LoginAsync(browser, "admin", "admin");

        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        var cookie = Assert.Single(antwort.Headers.GetValues("Set-Cookie"), c => c.StartsWith("av.anmeldung="));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [SicherheitFact]
    public async Task Logout_beendet_die_Sitzung()
    {
        using var browser = NeuerBrowser();
        await LoginAsync(browser, "sabine", "sommer2014");
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("api/kunden")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await browser.PostAsync("api/logout", null)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [SicherheitFact]
    public async Task Fehlgeschlagener_Login_setzt_kein_Cookie()
    {
        using var browser = NeuerBrowser();

        var antwort = await LoginAsync(browser, "admin", "falsch");

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.False(antwort.Headers.Contains("Set-Cookie"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [SicherheitFact]
    public async Task Passwort_beachtet_Gross_und_Kleinschreibung()
    {
        // Abweichung vom Legacy-System: Dort funktionierte "ADMIN" wegen der SQL-Collation
        using var browser = NeuerBrowser();

        var antwort = await LoginAsync(browser, "admin", "ADMIN");

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
    }

    [SicherheitFact]
    public async Task Legacy_Token_gewaehrt_keinen_Zugriff()
    {
        // Das alte "Token" ist nur Base64("admin:Admin") und darf nichts mehr freischalten
        using var browser = NeuerBrowser();
        browser.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YWRtaW46QWRtaW4=");

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [SicherheitFact]
    public async Task Gefaelschtes_Cookie_gewaehrt_keinen_Zugriff()
    {
        using var browser = NeuerBrowser(cookieSpeicher: false);
        browser.DefaultRequestHeaders.Add("Cookie", "av.anmeldung=CfDJ8manipuliert");

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
    }
}
