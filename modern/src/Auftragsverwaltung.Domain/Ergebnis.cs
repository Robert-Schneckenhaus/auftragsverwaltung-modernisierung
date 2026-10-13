namespace Auftragsverwaltung.Domain;

/// <summary>
/// Ergebnis eines Anwendungsfalls. Die API übersetzt es in HTTP-Statuscodes,
/// die Fachlogik kennt dadurch kein HTTP.
/// </summary>
public abstract record Ergebnis<T>
{
    public sealed record Erfolg(T Wert) : Ergebnis<T>;
    public sealed record NichtGefunden : Ergebnis<T>;
    public sealed record Fehler(string Meldung) : Ergebnis<T>;

    public static implicit operator Ergebnis<T>(T wert) => new Erfolg(wert);
}

public static class Ergebnis
{
    public static Ergebnis<T>.Fehler Fehler<T>(string meldung) => new(meldung);
    public static Ergebnis<T>.NichtGefunden NichtGefunden<T>() => new();
}

/// <summary>Ergebnis ohne Rückgabewert.</summary>
public readonly record struct Erledigt;
