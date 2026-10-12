using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;

namespace Auftragsverwaltung.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // JSON statt XML, auch wenn der Browser text/html anfragt
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Fehlerdetails immer ausgeben, hilft beim Support
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}
