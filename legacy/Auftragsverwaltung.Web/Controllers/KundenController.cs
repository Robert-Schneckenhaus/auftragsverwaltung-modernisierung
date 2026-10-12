using System;
using System.Linq;
using System.Web.Http;
using Auftragsverwaltung.Web.Models;

namespace Auftragsverwaltung.Web.Controllers
{
    [RoutePrefix("api/kunden")]
    public class KundenController : ApiController
    {
        private AvContext db = new AvContext();

        // GET api/kunden?suche=...&nurAktive=true
        [HttpGet, Route("")]
        public IHttpActionResult GetKunden(string suche = null, bool nurAktive = true)
        {
            try
            {
                var query = db.Kunden.AsQueryable();
                if (nurAktive)
                    query = query.Where(k => k.Aktiv);
                if (!string.IsNullOrEmpty(suche))
                    query = query.Where(k => k.Name.Contains(suche) || k.Ort.Contains(suche) || k.PLZ.StartsWith(suche));

                var liste = query.OrderBy(k => k.Name).ToList();
                return Ok(liste);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/kunden/10001
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetKunde(int id)
        {
            var kunde = db.Kunden.Find(id);
            if (kunde == null)
                return NotFound();

            var auftraege = db.Auftraege.Where(a => a.KdNr == id).OrderByDescending(a => a.Datum).ToList();

            return Ok(new
            {
                kunde.KdNr,
                kunde.Name,
                kunde.Strasse,
                kunde.PLZ,
                kunde.Ort,
                kunde.Telefon,
                kunde.Email,
                kunde.Bemerkung,
                kunde.Aktiv,
                kunde.AngelegtAm,
                Auftraege = auftraege.Select(a => new { a.AuftragsNr, a.Datum, a.Status, a.Beschreibung })
            });
        }

        // POST api/kunden
        [HttpPost, Route("")]
        public IHttpActionResult PostKunde(Kunde kunde)
        {
            if (kunde == null || string.IsNullOrWhiteSpace(kunde.Name))
                return BadRequest("Name fehlt");

            if (kunde.PLZ != null && kunde.PLZ.Length != 5)
                return BadRequest("PLZ muss 5-stellig sein");

            kunde.Aktiv = true;
            kunde.AngelegtAm = DateTime.Now;
            db.Kunden.Add(kunde);
            db.SaveChanges();

            return Ok(kunde);
        }

        // PUT api/kunden/10001
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult PutKunde(int id, Kunde daten)
        {
            var kunde = db.Kunden.Find(id);
            if (kunde == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(daten.Name))
                return BadRequest("Name fehlt");

            if (daten.PLZ != null && daten.PLZ.Length != 5)
                return BadRequest("PLZ muss 5-stellig sein");

            kunde.Name = daten.Name;
            kunde.Strasse = daten.Strasse;
            kunde.PLZ = daten.PLZ;
            kunde.Ort = daten.Ort;
            kunde.Telefon = daten.Telefon;
            kunde.Email = daten.Email;
            kunde.Bemerkung = daten.Bemerkung;
            db.SaveChanges();

            return Ok(kunde);
        }

        // DELETE api/kunden/10001  (wird nur deaktiviert, wegen Rechnungen)
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult DeleteKunde(int id)
        {
            var kunde = db.Kunden.Find(id);
            if (kunde == null)
                return NotFound();

            var offeneAuftraege = db.Auftraege.Count(a => a.KdNr == id && a.Status != "Abgerechnet" && a.Status != "Erledigt");
            if (offeneAuftraege > 0)
                return BadRequest("Kunde hat noch " + offeneAuftraege + " offene Aufträge");

            kunde.Aktiv = false;
            db.SaveChanges();
            return Ok();
        }
    }
}
