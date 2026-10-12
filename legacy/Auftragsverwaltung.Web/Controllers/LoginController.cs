using System;
using System.Linq;
using System.Text;
using System.Web.Http;
using Auftragsverwaltung.Web.Models;

namespace Auftragsverwaltung.Web.Controllers
{
    public class LoginDaten
    {
        public string Login { get; set; }
        public string Passwort { get; set; }
    }

    public class LoginController : ApiController
    {
        private AvContext db = new AvContext();

        // POST api/login
        [HttpPost, Route("api/login")]
        public IHttpActionResult Login(LoginDaten daten)
        {
            if (daten == null)
                return BadRequest("Keine Daten");

            var benutzer = db.Benutzer.FirstOrDefault(b => b.Login == daten.Login && b.Passwort == daten.Passwort);
            if (benutzer == null)
                return BadRequest("Login oder Passwort falsch");

            // "Token" für das Frontend, wird serverseitig nicht geprüft
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(benutzer.Login + ":" + benutzer.Rolle));

            return Ok(new { benutzer.Login, benutzer.Rolle, Token = token });
        }
    }
}
