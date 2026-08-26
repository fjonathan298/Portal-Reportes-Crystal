// ============================================================================
// Global.asax.cs - PUNTO DE ENTRADA DE LA APLICACION
// ============================================================================
// Este archivo se ejecuta UNA SOLA VEZ cuando la aplicacion arranca en IIS.
// Es el equivalente a "Main()" en una aplicacion de consola.
//
// Aqui se configuran las "reglas globales" del sitio:
// - Rutas (como se traducen las URLs a controladores)
// - Filtros globales (autorizacion, errores, etc.)
// - Bundles (agrupar CSS/JS - opcional)
//
// FLUJO: Usuario abre el sitio -> IIS carga la app -> Application_Start()
//        se ejecuta -> registra las rutas -> la app queda lista.
// ============================================================================

using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using PortalReportesCrystal.Filters;
using PortalReportesCrystal.Models;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Filtro global de auditoria: asegura SesionId por cada request
            // autenticado. Los eventos de negocio se emiten desde los controladores.
            GlobalFilters.Filters.Add(new AuditAttribute());

            // Servicios:
            //   1. Cargar cache persistido de App_Data
            //   2. Lanzar escaneo en BACKGROUND para detectar reportes nuevos
            //      o modificados. No bloquea la primera peticion.
            string appData = Server.MapPath("~/App_Data");
            CacheParametros.Inicializar(appData);
            EstadoReportes.Inicializar(appData);
            SapBoClient.Inicializar(appData);
            AuditoriaService.Inicializar(appData);
            CacheParametros.IniciarEscaneoBackground(ObtenerRaicesDeReportes());
        }

        // Devuelve todas las carpetas donde el portal busca .rpt:
        // (1) ~/Reportes/ del proyecto y (2) las raices declaradas en configuracion.json
        private IEnumerable<string> ObtenerRaicesDeReportes()
        {
            var lista = new List<string> { Server.MapPath("~/Reportes/") };

            string cfg = Server.MapPath("~/ReportesLocales/configuracion.json");
            if (File.Exists(cfg))
            {
                try
                {
                    var json = File.ReadAllText(cfg);
                    var conf = new JavaScriptSerializer().Deserialize<ConfiguracionRaices>(json);
                    if (conf != null && conf.Raices != null)
                    {
                        foreach (var r in conf.Raices)
                            if (r != null && !string.IsNullOrWhiteSpace(r.Ruta))
                                lista.Add(r.Ruta);
                    }
                }
                catch { /* si el JSON esta mal, seguimos con la raiz base */ }
            }
            return lista;
        }
    }
}
