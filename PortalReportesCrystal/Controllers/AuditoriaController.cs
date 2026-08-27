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

        // GET: /Auditoria/DashboardData
        [HttpGet]
        public ActionResult DashboardData()
        {
            if (!EsAdminAuditoria())
                return new HttpStatusCodeResult(403);

            var data = new Dictionary<string, object>();
            string connStr = System.Configuration.ConfigurationManager.AppSettings["Audit:ConnectionString"] ?? "";
            if (string.IsNullOrWhiteSpace(connStr))
            {
                data["error"] = "Connection string no configurada";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();

                    data["sesiones_24h"] = EjecutarEscalar(conn,
                        "SELECT COUNT(DISTINCT SesionId) FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME())");
                    data["eventos_24h"] = EjecutarEscalar(conn,
                        "SELECT COUNT(*) FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME())");
                    data["exportaciones_24h"] = EjecutarEscalar(conn,
                        "SELECT COUNT(*) FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME()) AND TipoEventoId IN (30,31,32,33,40)");
                    data["errores_24h"] = EjecutarEscalar(conn,
                        "SELECT COUNT(*) FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME()) AND TipoEventoId = 50");
                    data["accesos_denegados_24h"] = EjecutarEscalar(conn,
                        "SELECT COUNT(*) FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME()) AND TipoEventoId = 51");

                    data["sesiones_30d"] = EjecutarEscalar(conn,
                        "SELECT COUNT(*) FROM audit.Sesion WHERE InicioUtc >= DATEADD(DAY, -30, SYSUTCDATETIME())");
                    data["usuarios_30d"] = EjecutarEscalar(conn,
                        "SELECT COUNT(DISTINCT Usuario) FROM audit.Evento WHERE FechaUtc >= DATEADD(DAY, -30, SYSUTCDATETIME())");
                    data["reportes_30d"] = EjecutarEscalar(conn,
                        "SELECT COUNT(DISTINCT NombreReporte) FROM audit.Evento WHERE FechaUtc >= DATEADD(DAY, -30, SYSUTCDATETIME()) AND NombreReporte IS NOT NULL");

                    data["top_reportes"] = EjecutarLista(conn,
                        "SELECT TOP 10 NombreReporte, COUNT(*) AS Total FROM audit.Evento WHERE FechaUtc >= DATEADD(DAY, -30, SYSUTCDATETIME()) AND NombreReporte IS NOT NULL GROUP BY NombreReporte ORDER BY Total DESC");

                    data["top_usuarios"] = EjecutarLista(conn,
                        "SELECT TOP 10 Usuario, COUNT(*) AS Total FROM audit.Evento WHERE FechaUtc >= DATEADD(DAY, -30, SYSUTCDATETIME()) GROUP BY Usuario ORDER BY Total DESC");

                    data["dist_hora"] = EjecutarLista(conn,
                        "SELECT DATEPART(HOUR, DATEADD(HOUR, -6, FechaUtc)) AS Hora, COUNT(*) AS Total FROM audit.Evento WHERE FechaUtc >= DATEADD(HOUR, -24, SYSUTCDATETIME()) GROUP BY DATEPART(HOUR, DATEADD(HOUR, -6, FechaUtc)) ORDER BY Hora");
                }
            }
            catch (Exception ex)
            {
                data["error"] = "Error al consultar: " + ex.Message;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: /Auditoria/Interacciones
        // Vista para observar en tiempo real las interacciones capturadas.
        [Authorize]
        public ActionResult Interacciones()
        {
            if (!EsAdminAuditoria())
                return new HttpStatusCodeResult(403, "Se requiere membresia en grupo Audit:GruposAdmin");
            return View();
        }

        // GET: /Auditoria/InteraccionesSapbo?desdeEventoId=N&raiz=sapbo&cuid=...
        // Devuelve los ultimos eventos SAP BO (o de cualquier raiz si se indica).
        // Se usa para auto-refresh de la timeline desde el navegador.
        [HttpGet]
        public ActionResult InteraccionesSapbo(long desdeEventoId = 0, string raiz = null, string cuid = null, int limite = 50)
        {
            if (!EsAdminAuditoria())
                return new HttpStatusCodeResult(403);
            if (limite < 1 || limite > 200) limite = 50;

            var data = new Dictionary<string, object>();
            string connStr = System.Configuration.ConfigurationManager.AppSettings["Audit:ConnectionString"] ?? "";
            if (string.IsNullOrWhiteSpace(connStr))
            {
                data["error"] = "Connection string no configurada";
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var sb = new System.Text.StringBuilder();
                    sb.Append("SELECT TOP (@limite) e.EventoId, e.FechaLocal, e.Usuario, e.IpCliente, ");
                    sb.Append("t.Codigo AS Evento, t.Categoria, e.RaizId, e.NombreReporte, ");
                    sb.Append("e.Categoria AS CarpetaReporte, e.Formato, e.DuracionMs, e.HttpStatus, ");
                    sb.Append("e.MensajeError, e.PathReporte, e.UrlOrigen ");
                    sb.Append("FROM audit.Evento e ");
                    sb.Append("JOIN audit.EventoTipo t ON t.TipoEventoId = e.TipoEventoId ");
                    sb.Append("WHERE e.EventoId > @desde ");
                    if (!string.IsNullOrEmpty(raiz))  sb.Append("AND e.RaizId = @raiz ");
                    if (!string.IsNullOrEmpty(cuid))  sb.Append("AND e.PathReporte = @cuid ");
                    sb.Append("ORDER BY e.EventoId DESC");

                    var lista = new List<Dictionary<string, object>>();
                    long maxId = desdeEventoId;
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sb.ToString(), conn))
                    {
                        cmd.Parameters.AddWithValue("@limite", limite);
                        cmd.Parameters.AddWithValue("@desde", desdeEventoId);
                        if (!string.IsNullOrEmpty(raiz)) cmd.Parameters.AddWithValue("@raiz", raiz);
                        if (!string.IsNullOrEmpty(cuid)) cmd.Parameters.AddWithValue("@cuid", cuid);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var fila = new Dictionary<string, object>();
                                for (int i = 0; i < r.FieldCount; i++)
                                    fila[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                                lista.Add(fila);
                                long eid = Convert.ToInt64(fila["EventoId"]);
                                if (eid > maxId) maxId = eid;
                            }
                        }
                    }

                    // Adjuntar parametros de cada evento (Almacen, Pais, Fechas, CUID...)
                    if (lista.Count > 0)
                    {
                        var ids = string.Join(",", lista.Select(x => Convert.ToInt64(x["EventoId"]).ToString()));
                        string sqlParams = "SELECT EventoId, NombreParametro, ValorParametro " +
                                           "FROM audit.EventoParametro WHERE EventoId IN (" + ids + ")";
                        var mapa = new Dictionary<long, List<string>>();
                        using (var cmd2 = new System.Data.SqlClient.SqlCommand(sqlParams, conn))
                        using (var r2 = cmd2.ExecuteReader())
                        {
                            while (r2.Read())
                            {
                                long eid = r2.GetInt64(0);
                                string n = r2.IsDBNull(1) ? "" : r2.GetString(1);
                                string v = r2.IsDBNull(2) ? "" : r2.GetString(2);
                                if (!mapa.ContainsKey(eid)) mapa[eid] = new List<string>();
                                mapa[eid].Add(n + "=" + v);
                            }
                        }
                        foreach (var fila in lista)
                        {
                            long eid = Convert.ToInt64(fila["EventoId"]);
                            fila["Parametros"] = mapa.ContainsKey(eid) ? string.Join(" | ", mapa[eid]) : "";
                        }
                    }

                    data["eventos"] = lista;
                    data["ultimoEventoId"] = maxId;
                }
            }
            catch (Exception ex)
            {
                data["error"] = "Error: " + ex.Message;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private static object EjecutarEscalar(System.Data.SqlClient.SqlConnection conn, string sql)
        {
            using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
            {
                var result = cmd.ExecuteScalar();
                return result ?? 0;
            }
        }

        private static List<Dictionary<string, object>> EjecutarLista(System.Data.SqlClient.SqlConnection conn, string sql)
        {
            var lista = new List<Dictionary<string, object>>();
            using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    var fila = new Dictionary<string, object>();
                    for (int i = 0; i < r.FieldCount; i++)
                        fila[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                    lista.Add(fila);
                }
            }
            return lista;
        }

        // ---------------- Helpers ----------------

        private static bool EsAdminAuditoria()
        {
            var user = System.Web.HttpContext.Current.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated) return false;

            // Bypass explicito para desarrollo / pruebas (Audit:BypassAdminEnDev=true)
            string bypass = ConfigurationManager.AppSettings["Audit:BypassAdminEnDev"] ?? "false";
            if (string.Equals(bypass, "true", StringComparison.OrdinalIgnoreCase)) return true;

            // Usuarios individuales autorizados (Audit:UsuariosAdmin)
            string usuarios = ConfigurationManager.AppSettings["Audit:UsuariosAdmin"] ?? "";
            if (!string.IsNullOrWhiteSpace(usuarios) && !string.IsNullOrEmpty(user.Identity.Name))
            {
                var listaUsr = usuarios.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(u => u.Trim())
                                       .Where(u => !string.IsNullOrEmpty(u));
                foreach (var u in listaUsr)
                {
                    if (string.Equals(u, user.Identity.Name, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }

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
