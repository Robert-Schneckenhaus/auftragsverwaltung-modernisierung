using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Auftragsverwaltung.Sicherheitstests;

public sealed class ProxyFactAttribute : FactAttribute
{
    public ProxyFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AV_PROXY_URL")))
            Skip = "AV_PROXY_URL nicht gesetzt";
    }
}

/// <summary>
/// Der Proxy schützt auch Legacy-Endpunkte, die selbst keine Anmeldung kennen (ADR 0003).
/// In der CI steht hinter dem Proxy statt des echten Legacy-Systems eine Attrappe (LegacyAttrappe/).
/// </summary>
public class ProxyTests
{
    private static readonly JsonSerializerOptions WieFrontend = new() { PropertyNamingPolicy = null };

    private static HttpClient NeuerBrowser()
    {
        var basis = Environment.GetEnvironmentVariable("AV_PROXY_URL")!;
        if (!basis.EndsWith('/')) basis += "/";
        return new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() }) { BaseAddress = new Uri(basis) };
    }

    private static async Task<HttpClient> AngemeldetAlsAsync(string login, string passwort)
    {
        var browser = NeuerBrowser();
        var antwort = await browser.PostAsync("api/login",
            JsonContent.Create(new { Login = login, Passwort = passwort }, options: WieFrontend));
        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        return browser;
    }

    [ProxyFact]
    public async Task Frontend_ist_ohne_Anmeldung_erreichbar()
    {
        using var browser = NeuerBrowser();
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("index.html")).StatusCode);
    }

    [ProxyFact]
    public async Task Legacy_API_ist_ohne_Anmeldung_gesperrt()
    {
        using var browser = NeuerBrowser();

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/auftraege")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/rechnungen")).StatusCode);
    }

    [ProxyFact]
    public async Task Moderne_API_ist_ueber_den_Proxy_ohne_Anmeldung_gesperrt()
    {
        using var browser = NeuerBrowser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [ProxyFact]
    public async Task Nach_Login_erreicht_die_Anfrage_das_Legacy_System()
    {
        using var browser = await AngemeldetAlsAsync("sabine", "sommer2014");

        var antwort = await browser.GetAsync("api/auftraege");

        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        Assert.Contains("legacy-attrappe", await antwort.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("api/kunden")).StatusCode);
    }

    [ProxyFact]
    public async Task Rechnungen_nur_fuer_Admins()
    {
        // Im Legacy-System wurde das nur im Frontend durchgesetzt
        using var mitarbeiterin = await AngemeldetAlsAsync("sabine", "sommer2014");
        Assert.Equal(HttpStatusCode.Forbidden, (await mitarbeiterin.GetAsync("api/rechnungen")).StatusCode);

        using var admin = await AngemeldetAlsAsync("admin", "admin");
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("api/rechnungen")).StatusCode);
    }

    [ProxyFact]
    public async Task Nach_Logout_ist_die_Legacy_API_wieder_gesperrt()
    {
        using var browser = await AngemeldetAlsAsync("admin", "admin");
        Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("api/auftraege")).StatusCode);

        await browser.PostAsync("api/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("api/auftraege")).StatusCode);
    }
}
