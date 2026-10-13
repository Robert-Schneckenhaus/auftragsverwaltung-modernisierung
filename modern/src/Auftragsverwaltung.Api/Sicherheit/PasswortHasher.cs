using Auftragsverwaltung.Domain.Anmeldung;
using Microsoft.AspNetCore.Identity;

namespace Auftragsverwaltung.Api.Sicherheit;

/// <summary>
/// Nutzt den bewährten PasswordHasher von ASP.NET Core (PBKDF2 mit Salt) statt eines Eigenbaus.
/// </summary>
internal sealed class PasswortHasher : IPasswortHasher
{
    private readonly PasswordHasher<Benutzer> hasher = new();

    public string Hashen(string passwort) => hasher.HashPassword(null!, passwort);

    public PasswortPruefung Pruefen(string hash, string passwort)
        => hasher.VerifyHashedPassword(null!, hash, passwort) switch
        {
            PasswordVerificationResult.Success => PasswortPruefung.Richtig,
            PasswordVerificationResult.SuccessRehashNeeded => PasswortPruefung.RichtigNeuHashen,
            _ => PasswortPruefung.Falsch
        };
}
