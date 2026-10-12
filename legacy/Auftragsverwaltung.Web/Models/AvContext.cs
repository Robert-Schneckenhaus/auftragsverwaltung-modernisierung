using System.Data.Entity;

namespace Auftragsverwaltung.Web.Models
{
    public class AvContext : DbContext
    {
        static AvContext()
        {
            // Datenbank wird per Skript angelegt, EF soll nichts anfassen
            Database.SetInitializer<AvContext>(null);
        }

        public AvContext() : base("name=AvDb")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Kunde> Kunden { get; set; }
        public DbSet<Auftrag> Auftraege { get; set; }
        public DbSet<Position> Positionen { get; set; }
        public DbSet<Rechnung> Rechnungen { get; set; }
        public DbSet<Benutzer> Benutzer { get; set; }
    }
}
