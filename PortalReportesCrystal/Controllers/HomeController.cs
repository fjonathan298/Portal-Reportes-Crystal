// ============================================================================
// HomeController.cs - CONTROLADOR PRINCIPAL (la "C" de MVC)
// ============================================================================
// Consolida el listado de reportes desde tres fuentes:
//
//   1) CARPETA CLASICA DEL PROYECTO ~/Reportes/
//      Reportes de prueba historicos del portal. Se conservan por compatibilidad.
//
//   2) RAICES CONFIGURABLES en ReportesLocales/configuracion.json
//      Carpetas en disco (dentro o fuera del proyecto) que contienen .rpt.
//      Sus SUBCARPETAS de primer nivel se muestran como grupos/categorias.
//      Los .rpt sueltos en la raiz van a la categoria del "prefijoGrupoRaiz".
//
//   3) REPORTES EXTERNOS en ReportesCMC/catalogo.json
//      Enlaces a servidores externos (SAP BO CMC, etc.).
//
// Agregar o mover reportes de las fuentes 2 y 3 NO requiere recompilar:
// basta editar el JSON correspondiente y recargar la pagina.
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PortalReportesCrystal.Filters;
using PortalReportesCrystal.Models;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // Categoria por defecto para .rpt en ~/Reportes/ (sin agrupacion)
        private const string CATEGORIA_LEGACY = "Reportes locales";

        // Id reservado para la raiz clasica ~/Reportes/ del proyecto
        private const string RAIZ_LEGACY_ID = "proyecto";

        // GET: /  o  /Home/Index
        public ActionResult Index()
        {
            var reportes = new List<ReporteInfo>();

            reportes.AddRange(CargarReportesLegacy());
            reportes.AddRange(CargarReportesRaices());
            reportes.AddRange(CargarReportesCMC());
            reportes.AddRange(CargarReportesWebI());

            var ordenados = reportes
                .OrderBy(r => r.Categoria)
                .ThenBy(r => r.Nombre)
                .ToList();

            var model = new HomeViewModel
            {
                Reportes = ordenados,
                UsuarioActual = User.Identity.Name,
                WebIDesdeCache = SapBoClient.DatosDesdeCache,
                WebIUltimaActualizacion = SapBoClient.UltimaActualizacion,
                WebIHabilitado = SapBoClient.Habilitado
            };

            // Auditoria: apertura del listado principal
            try
            {
                if (AuditoriaService.Habilitado)
                {
                    AuditoriaService.RegistrarEvento(new EventoAuditoria
                    {
                        SesionId = AuditContext.SesionActual(HttpContext),
                        TipoEvento = "VER_LISTADO",
                        Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                        IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                        Parametros = new Dictionary<string, string>
                        {
                            { "TOTAL_REPORTES", ordenados.Count.ToString() }
                        }
                    });
                }
            }
            catch { }

            return View(model);
        }

        // GET: /Home/Estadisticas
        // Muestra estadisticas del servidor SAP BO en tiempo real (sesiones,
        // licencias, servidores) mas resumen de reportes desde el cache.
        public ActionResult Estadisticas()
        {
            var model = new EstadisticasViewModel
            {
                UsuarioActual = User.Identity.Name,
                DatosDesdeCache = SapBoClient.DatosDesdeCache,
                UltimoEscaneo = SapBoClient.UltimaActualizacion
            };

            if (SapBoClient.Habilitado)
            {
                try
                {
                    var reportes = SapBoClient.ObtenerReportes();
                    model.TotalCrystalReports = reportes.Count(r => r.TipoDocumento == "CrystalReport");
                    model.TotalWebI = reportes.Count(r => r.TipoDocumento == "WebI");
                    model.TotalReportes = reportes.Count;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Estadisticas/Reportes: {0}", ex.Message);
                }

                var sesiones = SapBoClient.ConsultarSesiones();
                model.Sesiones = sesiones.Items;
                model.SesionesError = sesiones.Error;

                var licencias = SapBoClient.ConsultarLicencias();
                model.Licencias = licencias.Items;
                model.LicenciasError = licencias.Error;

                var servidores = SapBoClient.ConsultarServidores();
                model.Servidores = servidores.Items;
                model.ServidoresError = servidores.Error;
            }

            return View(model);
        }

        // --------------------------------------------------------------------
        // Fuente 1: ~/Reportes/ (carpeta clasica del proyecto)
        // --------------------------------------------------------------------
        private List<ReporteInfo> CargarReportesLegacy()
        {
            string ruta = Server.MapPath("~/Reportes/");
            if (!Directory.Exists(ruta))
                return new List<ReporteInfo>();

            return Directory.GetFiles(ruta, "*.rpt")
                .Select(f =>
                {
                    string archivo = Path.GetFileName(f);
                    var est = EstadoReportes.Obtener(EstadoReportes.ClaveDeLocal(RAIZ_LEGACY_ID, archivo));
                    return new ReporteInfo
                    {
                        Nombre = Path.GetFileNameWithoutExtension(f),
                        Archivo = archivo,
                        RaizId = RAIZ_LEGACY_ID,
                        PathRelativo = archivo,
                        Categoria = CATEGORIA_LEGACY,
                        Tipo = TipoReporte.Local,
                        Servidor = "Local",
                        TieneParametros = CacheParametros.Analizar(f),
                        UltimoError = est != null && est.ConError ? est.Mensaje : null,
                        FechaUltimoError = est != null && est.ConError ? est.FechaIso : null
                    };
                })
                .ToList();
        }

        // --------------------------------------------------------------------
        // Fuente 2: raices declaradas en ReportesLocales/configuracion.json
        //
        // Estructura esperada por raiz:
        //   <raiz>/
        //     <Grupo1>/   -> reportes.rpt (categoria = "Grupo1")
        //     <Grupo2>/
        //     archivo.rpt -> categoria = prefijoGrupoRaiz
        //
        // Si el JSON tiene error, se loguea y se ignora esa raiz.
        // --------------------------------------------------------------------
        private List<ReporteInfo> CargarReportesRaices()
        {
            string cfgPath = Server.MapPath("~/ReportesLocales/configuracion.json");
            if (!System.IO.File.Exists(cfgPath))
                return new List<ReporteInfo>();

            ConfiguracionRaices cfg;
            try
            {
                string json = System.IO.File.ReadAllText(cfgPath);
                cfg = new JavaScriptSerializer().Deserialize<ConfiguracionRaices>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Error al leer ReportesLocales/configuracion.json: {0}", ex.Message);
                return new List<ReporteInfo>();
            }

            if (cfg == null || cfg.Raices == null)
                return new List<ReporteInfo>();

            var lista = new List<ReporteInfo>();
            foreach (var raiz in cfg.Raices)
            {
                if (string.IsNullOrWhiteSpace(raiz.Id) || string.IsNullOrWhiteSpace(raiz.Ruta))
                    continue;

                if (!Directory.Exists(raiz.Ruta))
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "Raiz de reportes '{0}' no encontrada en disco: {1}", raiz.Id, raiz.Ruta);
                    continue;
                }

                string catRaiz = string.IsNullOrWhiteSpace(raiz.PrefijoGrupoRaiz)
                    ? raiz.Nombre : raiz.PrefijoGrupoRaiz;

                try
                {
                    // Reportes sueltos en la raiz -> categoria del prefijo
                    foreach (var f in Directory.GetFiles(raiz.Ruta, "*.rpt"))
                    {
                        lista.Add(BuildReporte(raiz, catRaiz, Path.GetFileName(f)));
                    }

                    // Subcarpetas de primer nivel = grupos
                    foreach (var dir in Directory.GetDirectories(raiz.Ruta))
                    {
                        string grupo = Path.GetFileName(dir);
                        // GetFiles recursivo (por si el grupo tiene subgrupos)
                        foreach (var f in Directory.GetFiles(dir, "*.rpt", SearchOption.AllDirectories))
                        {
                            string relDesdeRaiz = f.Substring(raiz.Ruta.Length)
                                .TrimStart('\\', '/');
                            lista.Add(BuildReporte(raiz, grupo, relDesdeRaiz));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "Error al escanear raiz '{0}': {1}", raiz.Id, ex.Message);
                }
            }

            return lista;
        }

        private static ReporteInfo BuildReporte(RaizLocal raiz, string categoria, string pathRel)
        {
            string rutaAbs = Path.Combine(raiz.Ruta, pathRel);
            string pathRelNorm = pathRel.Replace('\\', '/');
            var estado = EstadoReportes.Obtener(EstadoReportes.ClaveDeLocal(raiz.Id, pathRelNorm));

            return new ReporteInfo
            {
                Nombre = Path.GetFileNameWithoutExtension(pathRel),
                Archivo = Path.GetFileName(pathRel),
                RaizId = raiz.Id,
                PathRelativo = pathRelNorm,
                Categoria = categoria,
                Tipo = TipoReporte.Local,
                Servidor = raiz.Nombre,
                TieneParametros = CacheParametros.Analizar(rutaAbs),
                UltimoError = estado != null && estado.ConError ? estado.Mensaje : null,
                FechaUltimoError = estado != null && estado.ConError ? estado.FechaIso : null
            };
        }

        // --------------------------------------------------------------------
        // Fuente 3: reportes externos declarados en ReportesCMC/catalogo.json
        // --------------------------------------------------------------------
        private List<ReporteInfo> CargarReportesCMC()
        {
            string ruta = Server.MapPath("~/ReportesCMC/catalogo.json");
            if (!System.IO.File.Exists(ruta))
                return new List<ReporteInfo>();

            try
            {
                string json = System.IO.File.ReadAllText(ruta);
                var catalogo = new JavaScriptSerializer().Deserialize<CatalogoCMC>(json);

                if (catalogo == null || catalogo.Grupos == null)
                    return new List<ReporteInfo>();

                var lista = new List<ReporteInfo>();
                foreach (var grupo in catalogo.Grupos)
                {
                    if (grupo.Reportes == null) continue;
                    foreach (var r in grupo.Reportes)
                    {
                        lista.Add(new ReporteInfo
                        {
                            Nombre = r.Nombre,
                            Descripcion = r.Descripcion,
                            Categoria = grupo.Nombre,
                            Tipo = TipoReporte.Externo,
                            Servidor = r.Servidor,
                            UrlExterna = r.Url
                        });
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Error al leer ReportesCMC/catalogo.json: {0}", ex.Message);
                return new List<ReporteInfo>();
            }
        }

        // --------------------------------------------------------------------
        // Fuente 4: reportes Web Intelligence descubiertos via API REST SAP BO
        // --------------------------------------------------------------------
        private List<ReporteInfo> CargarReportesWebI()
        {
            if (!SapBoClient.Habilitado)
                return new List<ReporteInfo>();

            try
            {
                return SapBoClient.ObtenerReportes()
                    .Select(w => new ReporteInfo
                    {
                        Nombre = w.Nombre,
                        Descripcion = w.Descripcion,
                        Categoria = w.Carpeta,
                        Tipo = TipoReporte.WebI,
                        Servidor = w.TipoDocumento == "CrystalReport" ? "SAP BO .rpt" : "SAP BO WebI",
                        UrlExterna = w.UrlOpenDocument
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Error al cargar reportes WebI desde API SAP BO: {0}", ex.Message);
                return new List<ReporteInfo>();
            }
        }
    }
}
