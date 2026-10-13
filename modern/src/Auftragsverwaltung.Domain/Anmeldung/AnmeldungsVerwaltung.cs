namespace Auftragsverwaltung.Domain.Anmeldung;

public interface IBenutzerRepository
{
    /// <summary>Sucht per Login; Groß-/Kleinschreibung des Logins richtet sich nach der Datenbank-Collation.</summary>
    Task<Benutzer?> LadenAsync(string login, CancellationToken ct);

    Task SpeichernAsync(CancellationToken ct);
}

public sealed record AnmeldeDaten(string? Login, string? Passwort);

public sealed class AnmeldungsVerwaltung(IBenutzerRepository repository, IPasswortHasher hasher)
{
    public const string FalscheAnmeldung = "Login oder Passwort falsch";

    // Hash, gegen den bei unbekannten Benutzern geprüft wird. So dauert die Antwort
    // gleich lang und verrät nicht, ob ein Login existiert.
    private static string? attrappenHash;

    public async Task<Ergebnis<Benutzer>> AnmeldenAsync(AnmeldeDaten? daten, CancellationToken ct = default)
    {
        if (daten is null)
            return Ergebnis.Fehler<Benutzer>("Keine Daten");

        var benutzer = string.IsNullOrEmpty(daten.Login) ? null : await repository.LadenAsync(daten.Login, ct);
        var passwort = daten.Passwort ?? "";

        if (benutzer is null)
        {
            attrappenHash ??= hasher.Hashen(Guid.NewGuid().ToString());
            hasher.Pruefen(attrappenHash, passwort);
            return Ergebnis.Fehler<Benutzer>(FalscheAnmeldung);
        }

        if (!benutzer.PasswortPruefen(passwort, hasher))
            return Ergebnis.Fehler<Benutzer>(FalscheAnmeldung);

        await repository.SpeichernAsync(ct); // speichert ggf. den neuen Hash
        return benutzer;
    }
}
