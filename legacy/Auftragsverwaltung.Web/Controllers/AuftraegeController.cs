using System;
using System.Linq;
using System.Web.Http;
using Auftragsverwaltung.Web.Models;

namespace Auftragsverwaltung.Web.Controllers
{
    public class StatusDaten
    {
        public string Status { get; set; }
        public string Techniker { get; set; }
    }

    [RoutePrefix("api/auftraege")]
    public class AuftraegeController : ApiController
    {
        private AvContext db = new AvContext();

        private static readonly string[] ErlaubteStatus = { "Offen", "In Arbeit", "Erledigt", "Abgerechnet" };

        // GET api/auftraege?status=Offen&kdnr=10002
        [HttpGet, Route("")]
        public IHttpActionResult GetAuftraege(string status = null, int? kdnr = null)
        {
            try
            {
                var query = db.Auftraege.Include("Kunde").Include("Positionen").AsQueryable();
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status);
                if (kdnr.HasValue)
                    query = query.Where(a => a.KdNr == kdnr.Value);

                var liste = query.OrderByDescending(a => a.Datum).ToList();

                return Ok(liste.Select(a => new
                {
                    a.AuftragsNr,
                    a.KdNr,
                    Kundenname = a.Kunde.Name,
                    a.Datum,
                    a.Status,
                    a.Beschreibung,
                    a.Techniker,
                    Summe = a.Positionen.Sum(p => p.Menge * p.Einzelpreis)
                }));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/auftraege/2014001
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetAuftrag(int id)
        {
            var a = db.Auftraege.Include("Kunde").Include("Positionen").FirstOrDefault(x => x.AuftragsNr == id);
            if (a == null)
                return NotFound();

            var rechnung = db.Rechnungen.FirstOrDefault(r => r.AuftragsNr == id);

            return Ok(new
            {
                a.AuftragsNr,
                a.KdNr,
                Kundenname = a.Kunde.Name,
                Kundenadresse = a.Kunde.Strasse + ", " + a.Kunde.PLZ + " " + a.Kunde.Ort,
                a.Datum,
                a.Status,
                a.Beschreibung,
                a.Techniker,
                Positionen = a.Positionen.Select(p => new
                {
                    p.PosId,
                    p.Bezeichnung,
                    p.Menge,
                    p.Einheit,
                    p.Einzelpreis,
                    Gesamt = p.Menge * p.Einzelpreis
                }),
                Summe = a.Positionen.Sum(p => p.Menge * p.Einzelpreis),
                RechNr = rechnung != null ? (int?)rechnung.RechNr : null
            });
        }

        // POST api/auftraege
        [HttpPost, Route("")]
        public IHttpActionResult PostAuftrag(Auftrag auftrag)
        {
            if (auftrag == null)
                return BadRequest("Keine Daten");

            var kunde = db.Kunden.Find(auftrag.KdNr);
            if (kunde == null || !kunde.Aktiv)
                return BadRequest("Kunde nicht gefunden oder inaktiv");

            auftrag.Datum = DateTime.Now;
            auftrag.Status = "Offen";
            db.Auftraege.Add(auftrag);
            db.SaveChanges();

            return Ok(new { auftrag.AuftragsNr });
        }

        // PUT api/auftraege/2014004/status
        [HttpPut, Route("{id:int}/status")]
        public IHttpActionResult PutStatus(int id, StatusDaten daten)
        {
            var auftrag = db.Auftraege.Find(id);
            if (auftrag == null)
                return NotFound();

            if (auftrag.Status == "Abgerechnet")
                return BadRequest("Abgerechnete Aufträge können nicht mehr geändert werden");

            if (daten == null || !ErlaubteStatus.Contains(daten.Status))
                return BadRequest("Ungültiger Status");

            if (daten.Status == "Abgerechnet")
                return BadRequest("Bitte Rechnung erstellen");

            if (daten.Status == "In Arbeit" && string.IsNullOrEmpty(daten.Techniker) && string.IsNullOrEmpty(auftrag.Techniker))
                return BadRequest("Techniker muss angegeben werden");

            auftrag.Status = daten.Status;
            if (!string.IsNullOrEmpty(daten.Techniker))
                auftrag.Techniker = daten.Techniker;

            db.SaveChanges();
            return Ok();
        }

        // POST api/auftraege/2014003/positionen
        [HttpPost, Route("{id:int}/positionen")]
        public IHttpActionResult PostPosition(int id, Position pos)
        {
            var auftrag = db.Auftraege.Find(id);
            if (auftrag == null)
                return NotFound();

            if (auftrag.Status == "Abgerechnet")
                return BadRequest("Auftrag ist bereits abgerechnet");

            if (pos == null || string.IsNullOrWhiteSpace(pos.Bezeichnung))
                return BadRequest("Bezeichnung fehlt");

            if (pos.Menge <= 0)
                return BadRequest("Menge muss größer 0 sein");

            pos.AuftragsNr = id;
            db.Positionen.Add(pos);
            db.SaveChanges();

            return Ok(new { pos.PosId });
        }

        // DELETE api/auftraege/2014003/positionen/9
        [HttpDelete, Route("{id:int}/positionen/{posId:int}")]
        public IHttpActionResult DeletePosition(int id, int posId)
        {
            var auftrag = db.Auftraege.Find(id);
            if (auftrag == null)
                return NotFound();

            if (auftrag.Status == "Abgerechnet")
                return BadRequest("Auftrag ist bereits abgerechnet");

            var pos = db.Positionen.FirstOrDefault(p => p.PosId == posId && p.AuftragsNr == id);
            if (pos == null)
                return NotFound();

            db.Positionen.Remove(pos);
            db.SaveChanges();
            return Ok();
        }

        // POST api/auftraege/2014002/rechnung
        [HttpPost, Route("{id:int}/rechnung")]
        public IHttpActionResult PostRechnung(int id)
        {
            var auftrag = db.Auftraege.Include("Positionen").FirstOrDefault(a => a.AuftragsNr == id);
            if (auftrag == null)
                return NotFound();

            if (auftrag.Status != "Erledigt")
                return BadRequest("Nur erledigte Aufträge können abgerechnet werden");

            if (!auftrag.Positionen.Any())
                return BadRequest("Auftrag hat keine Positionen");

            if (db.Rechnungen.Any(r => r.AuftragsNr == id))
                return BadRequest("Rechnung existiert bereits");

            var netto = auftrag.Positionen.Sum(p => p.Menge * p.Einzelpreis);
            netto = Math.Round(netto, 2);
            var mwst = Math.Round(netto * 0.19m, 2); // MwSt-Satz fest, siehe 2007
            var brutto = netto + mwst;

            var rechnung = new Rechnung
            {
                AuftragsNr = id,
                Datum = DateTime.Now,
                Netto = netto,
                MwSt = mwst,
                Brutto = brutto,
                Bezahlt = false
            };
            db.Rechnungen.Add(rechnung);
            auftrag.Status = "Abgerechnet";
            db.SaveChanges();

            return Ok(new { rechnung.RechNr, rechnung.Netto, rechnung.MwSt, rechnung.Brutto });
        }
    }
}
