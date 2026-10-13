using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Auftragsverwaltung.Charakterisierung;

/// <summary>
/// Hält fest, wie sich api/kunden im Legacy-System verhält – auch dort, wo es fragwürdig ist.
/// Grundlage sind die Testdaten aus legacy/Datenbank/02_testdaten.sql.
/// </summary>
public class KundenTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly string[] KundeFelder =
        ["KdNr", "Name", "Strasse", "PLZ", "Ort", "Telefon", "Email", "Bemerkung", "Aktiv", "AngelegtAm", "Auftraege"];

    // Das AngularJS-Frontend sendet PascalCase, genau so senden wir auch
    private static readonly JsonSerializerOptions WieFrontend = new() { PropertyNamingPolicy = null };

    private HttpClient Http => api.Client;

    private static object NeuerKunde(string? name = null, string? plz = "79098") => new
    {
        Name = name ?? $"Charakterisierung {Guid.NewGuid():N}",
        Strasse = "Teststraße 1",
        PLZ = plz,
        Ort = "Teststadt",
        Telefon = "0761 000000",
        Email = "test@example.org",
        Bemerkung = "angelegt durch Charakterisierungstest"
    };

    private Task<HttpResponseMessage> PostAsync(string url, object daten)
        => Http.PostAsync(url, JsonContent.Create(daten, options: WieFrontend));

    private Task<HttpResponseMessage> PutAsync(string url, object daten)
        => Http.PutAsync(url, JsonContent.Create(daten, options: WieFrontend));

    private async Task<JsonElement> KundeAnlegenAsync()
    {
        var antwort = await PostAsync("api/kunden", NeuerKunde());
        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        return await antwort.JsonAsync();
    }

    private async Task<List<string?>> NamenAsync(string url)
    {
        var liste = await Http.GetFromJsonAsync<JsonElement>(url);
        return liste.EnumerateArray().Select(k => k.Text("Name")).ToList();
    }

    // ---------- Liste und Suche ----------

    [CharakterisierungFact]
    public async Task Liste_enthaelt_nur_aktive_Kunden_sortiert_nach_Name()
    {
        var namen = await NamenAsync("api/kunden");

        string[] erwartet = ["Bäckerei Hofmann", "Dr. Petra Wagner", "Familie Brandt", "Gasthaus Zum Adler", "Hausverwaltung Kessler"];
        Assert.Equal(erwartet, namen.Where(erwartet.Contains));
        Assert.DoesNotContain("Autohaus Rieger (alt)", namen);
    }

    [CharakterisierungFact]
    public async Task Liste_mit_nurAktive_false_enthaelt_inaktive_Kunden()
        => Assert.Contains("Autohaus Rieger (alt)", await NamenAsync("api/kunden?nurAktive=false"));

    [CharakterisierungFact]
    public async Task Suche_nach_Ort_ignoriert_Gross_und_Kleinschreibung()
    {
        var namen = await NamenAsync("api/kunden?suche=FREIBURG");

        Assert.Contains("Bäckerei Hofmann", namen);
        Assert.Contains("Hausverwaltung Kessler", namen);
        Assert.Contains("Dr. Petra Wagner", namen);
        Assert.DoesNotContain("Familie Brandt", namen);
        Assert.DoesNotContain("Autohaus Rieger (alt)", namen);
    }

    [CharakterisierungFact]
    public async Task Suche_nach_Name_findet_Teilstrings()
        => Assert.Equal(new[] { "Gasthaus Zum Adler" }, await NamenAsync("api/kunden?suche=adler"));

    [CharakterisierungFact]
    public async Task Suche_nach_PLZ_prueft_nur_den_Anfang()
    {
        Assert.Equal(new[] { "Familie Brandt" }, await NamenAsync("api/kunden?suche=792"));
        Assert.Empty(await NamenAsync("api/kunden?suche=211"));
    }

    [CharakterisierungFact]
    public async Task Listeneintraege_haben_exakt_das_Legacy_Format()
    {
        var liste = await Http.GetFromJsonAsync<JsonElement>("api/kunden?suche=Hofmann");
        var kunde = liste.EnumerateArray().Single();

        Assert.Equal(KundeFelder, kunde.Eigenschaften());
        Assert.Equal(10001, kunde.GetProperty("KdNr").GetInt32());
        Assert.Equal("79098", kunde.Text("PLZ"));
        Assert.True(kunde.GetProperty("Aktiv").GetBoolean());
        Assert.Equal(JsonValueKind.Null, kunde.GetProperty("Auftraege").ValueKind);
    }

    // ---------- Detail ----------

    [CharakterisierungFact]
    public async Task Detail_enthaelt_Auftraege_neueste_zuerst_mit_Originalstatus()
    {
        var kunde = await Http.GetFromJsonAsync<JsonElement>("api/kunden/10002");

        Assert.Equal(KundeFelder, kunde.Eigenschaften());
        Assert.Equal("Hausverwaltung Kessler", kunde.Text("Name"));

        var auftraege = kunde.GetProperty("Auftraege").EnumerateArray().ToList();
        Assert.Equal(new[] { 2014006, 2014002 }, auftraege.Select(a => a.GetProperty("AuftragsNr").GetInt32()));
        Assert.Equal(new[] { "AuftragsNr", "Datum", "Status", "Beschreibung" }, auftraege[0].Eigenschaften());

        // Freitext-Status wird unverändert durchgereicht, auch in Kleinschreibung
        Assert.Equal("offen", auftraege[0].Text("Status"));
        Assert.Equal("Erledigt", auftraege[1].Text("Status"));
    }

    [CharakterisierungFact]
    public async Task Detail_eines_unbekannten_Kunden_ergibt_404()
        => Assert.Equal(HttpStatusCode.NotFound, (await Http.GetAsync("api/kunden/99999")).StatusCode);

    // ---------- Anlegen ----------

    [CharakterisierungFact]
    public async Task Anlegen_liefert_den_neuen_Kunden_aktiv_und_mit_Nummer()
    {
        var kunde = await KundeAnlegenAsync();

        Assert.Equal(KundeFelder, kunde.Eigenschaften());
        Assert.True(kunde.GetProperty("KdNr").GetInt32() > 10006);
        Assert.True(kunde.GetProperty("Aktiv").GetBoolean());
        Assert.True(kunde.GetProperty("AngelegtAm").GetDateTime() > new DateTime(2020, 1, 1));
        Assert.Equal("Teststadt", kunde.Text("Ort"));
    }

    [CharakterisierungFact]
    public async Task Anlegen_ignoriert_Aktiv_aus_dem_Request()
    {
        var antwort = await PostAsync("api/kunden", new { Name = $"Charakterisierung {Guid.NewGuid():N}", Aktiv = false });

        Assert.True((await antwort.JsonAsync()).GetProperty("Aktiv").GetBoolean());
    }

    [CharakterisierungFact]
    public async Task Anlegen_ohne_Name_ergibt_400_mit_Meldung()
    {
        var antwort = await PostAsync("api/kunden", new { Name = "  ", PLZ = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Name fehlt", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Anlegen_mit_leerem_Body_ergibt_400_Name_fehlt()
    {
        var antwort = await PostAsync("api/kunden", new { });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Name fehlt", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Anlegen_mit_Schweizer_PLZ_wird_abgelehnt()
    {
        var antwort = await PostAsync("api/kunden", NeuerKunde(plz: "8001"));

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("PLZ muss 5-stellig sein", await antwort.FehlermeldungAsync());
    }

    // ---------- Ändern ----------

    [CharakterisierungFact]
    public async Task Aendern_uebernimmt_Daten_und_liefert_Kunden()
    {
        var kdNr = (await KundeAnlegenAsync()).GetProperty("KdNr").GetInt32();
        var neuerName = $"Charakterisierung geändert {Guid.NewGuid():N}";

        var antwort = await PutAsync($"api/kunden/{kdNr}", NeuerKunde(name: neuerName, plz: "79100"));

        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        var kunde = await antwort.JsonAsync();
        Assert.Equal(KundeFelder, kunde.Eigenschaften());
        Assert.Equal(neuerName, kunde.Text("Name"));

        var geladen = await Http.GetFromJsonAsync<JsonElement>($"api/kunden/{kdNr}");
        Assert.Equal("79100", geladen.Text("PLZ"));
    }

    [CharakterisierungFact]
    public async Task Aendern_ohne_Name_ergibt_400()
    {
        var kdNr = (await KundeAnlegenAsync()).GetProperty("KdNr").GetInt32();

        var antwort = await PutAsync($"api/kunden/{kdNr}", new { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Name fehlt", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Aendern_eines_unbekannten_Kunden_ergibt_404()
        => Assert.Equal(HttpStatusCode.NotFound, (await PutAsync("api/kunden/99999", NeuerKunde())).StatusCode);

    // ---------- Deaktivieren ----------

    [CharakterisierungFact]
    public async Task Deaktivieren_mit_offenem_Auftrag_ergibt_400()
    {
        // Kunde 10002 hat Auftrag 2014006 mit Status "offen" (klein geschrieben) – zählt als offen
        var antwort = await Http.DeleteAsync("api/kunden/10002");

        Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        Assert.Equal("Kunde hat noch 1 offene Aufträge", await antwort.FehlermeldungAsync());
    }

    [CharakterisierungFact]
    public async Task Deaktivieren_ohne_Auftraege_blendet_Kunden_aus_der_Liste_aus()
    {
        var kunde = await KundeAnlegenAsync();
        var kdNr = kunde.GetProperty("KdNr").GetInt32();
        var name = kunde.Text("Name");

        var antwort = await Http.DeleteAsync($"api/kunden/{kdNr}");

        Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
        Assert.DoesNotContain(name, await NamenAsync("api/kunden"));
        Assert.Contains(name, await NamenAsync("api/kunden?nurAktive=false"));

        var geladen = await Http.GetFromJsonAsync<JsonElement>($"api/kunden/{kdNr}");
        Assert.False(geladen.GetProperty("Aktiv").GetBoolean());
    }

    [CharakterisierungFact]
    public async Task Deaktivieren_eines_unbekannten_Kunden_ergibt_404()
        => Assert.Equal(HttpStatusCode.NotFound, (await Http.DeleteAsync("api/kunden/99999")).StatusCode);
}
