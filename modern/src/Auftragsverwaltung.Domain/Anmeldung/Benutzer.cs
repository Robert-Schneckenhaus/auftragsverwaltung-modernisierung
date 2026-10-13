using System.Security.Cryptography;
using System.Text;

namespace Auftragsverwaltung.Domain.Anmeldung;

public enum PasswortPruefung
{
    Falsch,
    Richtig,
    /// <summary>Richtig, aber mit veraltetem Verfahren gehasht.</summary>
    RichtigNeuHashen
}

/// <summary>Hashverfahren, umgesetzt außerhalb der Fachlogik (ASP.NET Core PasswordHasher).</summary>
public interface IPasswortHasher
{
    string Hashen(string passwort);
    PasswortPruefung Pruefen(string hash, string passwort);
}

public static class Rollen
{
    public const string Admin = "Admin";
}

public sealed class Benutzer
{
    public int Id { get; private set; }
    public string Login { get; private set; } = "";

    /// <summary>
    /// Klartextpasswort aus dem Legacy-System. Wird beim ersten erfolgreichen Login
    /// durch <see cref="PasswortHash"/> ersetzt und danach geleert (ADR 0003).
    /// </summary>
    public string Passwort { get; private set; } = "";

    public string? PasswortHash { get; private set; }
    public string Rolle { get; private set; } = "";

    private Benutzer() { } // für EF Core

    /// <summary>Nur für Tests und Datenübernahmen.</summary>
    public static Benutzer AusLegacy(string login, string klartextPasswort, string rolle)
        => new() { Login = login, Passwort = klartextPasswort, Rolle = rolle };

    public bool KlartextVorhanden => PasswortHash is null && Passwort.Length > 0;

    /// <summary>
    /// Prüft das Passwort und stellt dabei transparent auf einen Hash um.
    /// Abweichung vom Legacy-System: Groß-/Kleinschreibung wird beachtet.
    /// </summary>
    public bool PasswortPruefen(string passwort, IPasswortHasher hasher)
    {
        if (PasswortHash is not null)
        {
            var ergebnis = hasher.Pruefen(PasswortHash, passwort);
            if (ergebnis == PasswortPruefung.RichtigNeuHashen)
                PasswortHash = hasher.Hashen(passwort);
            return ergebnis != PasswortPruefung.Falsch;
        }

        if (!KlartextVorhanden || !GleichInKonstanterZeit(Passwort, passwort))
            return false;

        PasswortHash = hasher.Hashen(passwort);
        Passwort = "";
        return true;
    }

    private static bool GleichInKonstanterZeit(string a, string b)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
