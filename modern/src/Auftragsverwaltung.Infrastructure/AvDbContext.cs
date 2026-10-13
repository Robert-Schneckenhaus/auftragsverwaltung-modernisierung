using Auftragsverwaltung.Domain.Kunden;
using Microsoft.EntityFrameworkCore;

namespace Auftragsverwaltung.Infrastructure;

/// <summary>
/// EF Core auf dem bestehenden Legacy-Schema (siehe ADR 0002).
/// Tabellen- und Spaltennamen bleiben unverändert, das Domain-Modell bekommt saubere Namen.
/// </summary>
public sealed class AvDbContext(DbContextOptions<AvDbContext> options) : DbContext(options)
{
    public DbSet<Kunde> Kunden => Set<Kunde>();
    internal DbSet<AuftragZeile> Auftraege => Set<AuftragZeile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kunde>(e =>
        {
            e.ToTable("tblKunde");
            e.HasKey(k => k.KdNr);
            e.Property(k => k.KdNr).UseIdentityColumn();
            e.Property(k => k.Name).HasMaxLength(200).IsRequired();
            e.Property(k => k.Strasse).HasMaxLength(200);
            e.Property(k => k.Plz).HasColumnName("PLZ").HasMaxLength(10);
            e.Property(k => k.Ort).HasMaxLength(100);
            e.Property(k => k.Telefon).HasMaxLength(50);
            e.Property(k => k.Email).HasMaxLength(200);
            e.Property(k => k.AngelegtAm).HasColumnType("datetime");
        });

        // Vorerst nur lesend für die Kundenansicht. Das Modul Aufträge folgt in einem eigenen Schritt.
        modelBuilder.Entity<AuftragZeile>(e =>
        {
            e.ToTable("tblAuftrag");
            e.HasKey(a => a.AuftragsNr);
            e.Property(a => a.Datum).HasColumnType("datetime");
            e.Property(a => a.Status).HasMaxLength(50);
            e.Property(a => a.Techniker).HasMaxLength(100);
        });
    }
}

internal sealed class AuftragZeile
{
    public int AuftragsNr { get; set; }
    public int KdNr { get; set; }
    public DateTime Datum { get; set; }
    public string Status { get; set; } = "";
    public string? Beschreibung { get; set; }
    public string? Techniker { get; set; }
}
