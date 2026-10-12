using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auftragsverwaltung.Web.Models
{
    [Table("tblKunde")]
    public class Kunde
    {
        [Key]
        public int KdNr { get; set; }
        public string Name { get; set; }
        public string Strasse { get; set; }
        public string PLZ { get; set; }
        public string Ort { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public string Bemerkung { get; set; }
        public bool Aktiv { get; set; }
        public DateTime AngelegtAm { get; set; }

        public virtual ICollection<Auftrag> Auftraege { get; set; }
    }

    [Table("tblAuftrag")]
    public class Auftrag
    {
        [Key]
        public int AuftragsNr { get; set; }
        public int KdNr { get; set; }
        public DateTime Datum { get; set; }
        public string Status { get; set; }
        public string Beschreibung { get; set; }
        public string Techniker { get; set; }

        [ForeignKey("KdNr")]
        public virtual Kunde Kunde { get; set; }
        public virtual ICollection<Position> Positionen { get; set; }
    }

    [Table("tblPosition")]
    public class Position
    {
        [Key]
        public int PosId { get; set; }
        public int AuftragsNr { get; set; }
        public string Bezeichnung { get; set; }
        public decimal Menge { get; set; }
        public string Einheit { get; set; }
        public decimal Einzelpreis { get; set; }

        [ForeignKey("AuftragsNr")]
        public virtual Auftrag Auftrag { get; set; }
    }

    [Table("tblRechnung")]
    public class Rechnung
    {
        [Key]
        public int RechNr { get; set; }
        public int AuftragsNr { get; set; }
        public DateTime Datum { get; set; }
        public decimal Netto { get; set; }
        public decimal MwSt { get; set; }
        public decimal Brutto { get; set; }
        public bool Bezahlt { get; set; }

        [ForeignKey("AuftragsNr")]
        public virtual Auftrag Auftrag { get; set; }
    }

    [Table("tblBenutzer")]
    public class Benutzer
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Passwort { get; set; }
        public string Rolle { get; set; }
    }
}
