// ============================================================================
// AuditoriaController.cs - ENDPOINTS DE AUDITORIA DEL LADO CLIENTE
// ============================================================================
// Recibe eventos POST-JSON emitidos por JavaScript del portal:
//   - Descargas detectadas dentro del iframe SAP BO
//   - Diagnostico del test de iframe
//   - (futuro) Interacciones adicionales del cliente
//
// Tambien alberga el dashboard admin (Auditoria/Dashboard) que se restringe
// por membresia a los grupos AD definidos en Web.config (Audit:GruposAdmin).
// ============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PortalReportesCrystal.Filters;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal.Controllers
{
    [Authorize]
    public class AuditoriaController : Controller
    {
        // POST: /Auditoria/RegistrarInteraccion
        // Body JSON: { "tipo": "DESCARGA_IFRAME", "cuid": "...", "formato": "pdf" }
        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ActionResult RegistrarInteraccion()
        {
            try
            {
                if (!AuditoriaService.Habilitado) return new HttpStatusCodeResult(204);

                var body = LeerJson(Request);
                string tipo = body != null && body.ContainsKey("tipo") ? (body["tipo"] as string) : null;
                string cuid = body != null && body.ContainsKey("cuid") ? (body["cuid"] as string) : null;
                string formato = body != null && body.ContainsKey("formato") ? (body["formato"] as string) : null;

                if (string.IsNullOrWhiteSpace(tipo))
                    return new HttpStatusCodeResult(400);

                AuditoriaService.RegistrarEvento(new EventoAuditoria
                {
                    SesionId = AuditContext.SesionActual(HttpContext),
                    TipoEvento = tipo.ToUpperInvariant(),
                    Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                    IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                    RaizId = "sapbo",
                    PathReporte = cuid,
                    Formato = formato,
                    TipoReporte = "Sapbo"
                });

                return new HttpStatusCodeResult(204);
            }
            catch
            {
                return new HttpStatusCodeResult(500);
            }
        }

        // POST: /Auditoria/RegistrarDiagnosticoIframe
        // Body JSON: { "estado": "ok|warn|error", "mensaje": "...", "url": "..." }
        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ActionResult RegistrarDiagnosticoIframe()
        {
            try
            {
                var body = LeerJson(Request);
                string estado = body != null && body.ContainsKey("estado") ? (body["estado"] as string) : "";
                string mensaje = body != null && body.ContainsKey("mensaje") ? (body["mensaje"] as string) : "";
                string url = body != null && body.ContainsKey("url") ? (body["url"] as string) : "";

                var parametros = new Dictionary<string, string>
                {
                    { "ESTADO", estado ?? "" },
                    { "URL", url ?? "" }
                };

                if (AuditoriaService.Habilitado)
                {
                    AuditoriaService.RegistrarEvento(new EventoAuditoria
                    {
                        SesionId = AuditContext.SesionActual(HttpContext),
                        TipoEvento = "HEARTBEAT",   // no crear un tipo especifico para prueba de diagnostico
                        Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                        IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                        MensajeError = "TestIframe " + estado + ": " + (mensaje ?? ""),
                        Parametros = parametros
                    });
                }

                return new HttpStatusCodeResult(204);
            }
            catch
            {
                return new HttpStatusCodeResult(500);
            }
        }

        // GET: /Auditoria/Dashboard
        // Muestra tarjetas de resumen (ultimas 24h, ultimos 30d, top reportes/usuarios)
        [Authorize]
        public ActionResult Dashboard()
        {
            if (!EsAdminAuditoria())
                return new HttpStatusCodeResult(403, "Se requiere membresia en grupo Audit:GruposAdmin");

            // Registrar acceso al dashboard tambien
            try
            {
                if (AuditoriaService.Habilitado)
                {
                    AuditoriaService.RegistrarEvento(new EventoAuditoria
                    {
                        SesionId = AuditContext.SesionActual(HttpContext),
                        TipoEvento = "ACCESO_DASHBOARD",
                        Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                        IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current)
                    });
                }
            }
            catch { }

            return View();
        }

        // ---------------- Helpers ----------------

        private static bool EsAdminAuditoria()
        {
            var user = System.Web.HttpContext.Current.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated) return false;

            string grupos = ConfigurationManager.AppSettings["Audit:GruposAdmin"] ?? "";
            if (string.IsNullOrWhiteSpace(grupos)) return false;

            var partes = grupos.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(g => g.Trim())
                               .Where(g => !string.IsNullOrEmpty(g))
                               .ToArray();

            foreach (var grupo in partes)
            {
                try
                {
                    if (user.IsInRole(grupo)) return true;
                }
                catch { }
            }
            return false;
        }

        private static IDictionary<string, object> LeerJson(HttpRequestBase request)
        {
            if (request == null || request.InputStream == null) return null;
            try
            {
                request.InputStream.Position = 0;
                using (var reader = new System.IO.StreamReader(request.InputStream, System.Text.Encoding.UTF8))
                {
                    string body = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(body)) return null;
                    var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    return js.Deserialize<Dictionary<string, object>>(body);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
