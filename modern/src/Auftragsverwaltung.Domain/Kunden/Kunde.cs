namespace Auftragsverwaltung.Domain.Kunden;

/// <summary>Stammdaten, wie sie beim Anlegen und Ändern übergeben werden.</summary>
public sealed record KundenDaten(
    string? Name,
    string? Strasse,
    string? Plz,
    string? Ort,
    string? Telefon,
    string? Email,
    string? Bemerkung);

public sealed class Kunde
{
    public int KdNr { get; private set; }
    public string Name { get; private set; } = "";
    public string? Strasse { get; private set; }
    public string? Plz { get; private set; }
    public string? Ort { get; private set; }
    public string? Telefon { get; private set; }
    public string? Email { get; private set; }
    public string? Bemerkung { get; private set; }
    public bool Aktiv { get; private set; }
    public DateTime AngelegtAm { get; private set; }

    private Kunde() { } // für EF Core

    public static Ergebnis<Kunde> Anlegen(KundenDaten daten, DateTime jetzt)
    {
        if (Pruefen(daten) is { } fehler)
            return Ergebnis.Fehler<Kunde>(fehler);

        var kunde = new Kunde { Aktiv = true, AngelegtAm = jetzt };
        kunde.Uebernehmen(daten);
        return kunde;
    }

    public Ergebnis<Kunde> Aendern(KundenDaten daten)
    {
        if (Pruefen(daten) is { } fehler)
            return Ergebnis.Fehler<Kunde>(fehler);

        Uebernehmen(daten);
        return this;
    }

    /// <summary>
    /// Kunden werden nie gelöscht, sondern deaktiviert, weil Rechnungen auf sie verweisen.
    /// </summary>
    public Ergebnis<Erledigt> Deaktivieren(int anzahlOffeneAuftraege)
    {
        if (anzahlOffeneAuftraege > 0)
            return Ergebnis.Fehler<Erledigt>($"Kunde hat noch {anzahlOffeneAuftraege} offene Aufträge");

        Aktiv = false;
        return new Erledigt();
    }

    /// <summary>
    /// Validierung exakt wie im Legacy-System, inklusive seiner Eigenheiten:
    /// Eine leere PLZ ("") wird abgelehnt, null dagegen akzeptiert.
    /// Geprüft wird nur die Länge, Schweizer PLZ (4-stellig) sind damit nicht möglich.
    /// Änderungen daran sind fachliche Entscheidungen und brauchen einen ADR.
    /// </summary>
    private static string? Pruefen(KundenDaten daten)
    {
        if (string.IsNullOrWhiteSpace(daten.Name))
            return "Name fehlt";

        if (daten.Plz is not null && daten.Plz.Length != 5)
            return "PLZ muss 5-stellig sein";

        return null;
    }

    private void Uebernehmen(KundenDaten daten)
    {
        Name = daten.Name!;
        Strasse = daten.Strasse;
        Plz = daten.Plz;
        Ort = daten.Ort;
        Telefon = daten.Telefon;
        Email = daten.Email;
        Bemerkung = daten.Bemerkung;
    }
}
