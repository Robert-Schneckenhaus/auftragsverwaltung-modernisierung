using System.Linq;
using System.Web.Http;
using Auftragsverwaltung.Web.Models;

namespace Auftragsverwaltung.Web.Controllers
{
    [RoutePrefix("api/rechnungen")]
    public class RechnungenController : ApiController
    {
        private AvContext db = new AvContext();

        // GET api/rechnungen?offen=true
        [HttpGet, Route("")]
        public IHttpActionResult GetRechnungen(bool offen = false)
        {
            var query = db.Rechnungen.Include("Auftrag.Kunde").AsQueryable();
            if (offen)
                query = query.Where(r => !r.Bezahlt);

            var liste = query.OrderByDescending(r => r.Datum).ToList();

            return Ok(liste.Select(r => new
            {
                r.RechNr,
                r.AuftragsNr,
                Kundenname = r.Auftrag.Kunde.Name,
                r.Datum,
                r.Netto,
                r.MwSt,
                r.Brutto,
                r.Bezahlt
            }));
        }

        // GET api/rechnungen/500001
        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetRechnung(int id)
        {
            var r = db.Rechnungen.Include("Auftrag.Kunde").Include("Auftrag.Positionen").FirstOrDefault(x => x.RechNr == id);
            if (r == null)
                return NotFound();

            var k = r.Auftrag.Kunde;
            return Ok(new
            {
                r.RechNr,
                r.AuftragsNr,
                r.Datum,
                Empfaenger = new { k.KdNr, k.Name, k.Strasse, k.PLZ, k.Ort },
                Positionen = r.Auftrag.Positionen.Select(p => new
                {
                    p.Bezeichnung,
                    p.Menge,
                    p.Einheit,
                    p.Einzelpreis,
                    Gesamt = p.Menge * p.Einzelpreis
                }),
                r.Netto,
                r.MwSt,
                r.Brutto,
                r.Bezahlt
            });
        }

        // PUT api/rechnungen/500001/bezahlt
        [HttpPut, Route("{id:int}/bezahlt")]
        public IHttpActionResult PutBezahlt(int id)
        {
            var r = db.Rechnungen.Find(id);
            if (r == null)
                return NotFound();

            r.Bezahlt = true;
            db.SaveChanges();
            return Ok();
        }
    }
}
