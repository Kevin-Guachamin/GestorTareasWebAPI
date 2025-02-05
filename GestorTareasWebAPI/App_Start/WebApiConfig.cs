using System.Web.Http;
using System.Web.Http.Cors;

namespace GestorTareasWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilita CORS para todas las solicitudes desde 
            var cors = new EnableCorsAttribute("http://localhost:3000", "*", "GET,POST,PUT,DELETE");
            config.EnableCors(cors);


            // Configuración de rutas de Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
