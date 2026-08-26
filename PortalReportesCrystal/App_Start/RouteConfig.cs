// ============================================================================
// RouteConfig.cs - CONFIGURACION DE RUTAS (URLs)
// ============================================================================
// Este archivo define COMO se traducen las URLs del navegador a codigo C#.
//
// En MVC, una URL tiene esta estructura:
//   http://localhost/Controlador/Accion/Parametro
//
// Ejemplos con este proyecto:
//   http://localhost/              -> HomeController.Index()
//   http://localhost/Home/Index    -> HomeController.Index()
//   http://localhost/Reportes/Ver?archivo=ventas.rpt -> ReportesController.Ver("ventas.rpt")
//   http://localhost/Reportes/Exportar?archivo=ventas.rpt&formato=pdf
//                                  -> ReportesController.Exportar("ventas.rpt", "pdf")
//
// La ruta "Default" al final dice: si no se especifica controlador, usar "Home",
// si no se especifica accion, usar "Index". Por eso http://localhost/ va a Home/Index.
// ============================================================================

using System.Web.Mvc;
using System.Web.Routing;

namespace PortalReportesCrystal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Ignorar peticiones a archivos .axd (recursos internos de ASP.NET)
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Ruta por defecto: {controlador}/{accion}/{id opcional}
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
