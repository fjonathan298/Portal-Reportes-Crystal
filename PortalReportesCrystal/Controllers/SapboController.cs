// ============================================================================
// SapboController.cs - VISOR INTERNO DE REPORTES SAP BO
// ============================================================================
// Antes, los reportes SAP BO se abrian con target="_blank" fuera del portal,
// lo que impedia auditar quien los abria, con que filtros y si descargaba.
//
// Ahora todos los reportes SAP BO (WebI y Crystal publicados en el server)
// se abren DENTRO del portal a traves de este controlador:
//
//   /Sapbo/Ver?cuid=<CUID>&nombre=...     -> pagina wrapper con iframe
//   /Sapbo/Contenido?cuid=<CUID>          -> URL que el iframe carga
//   /Sapbo/TestIframe                     -> pagina de diagnostico
//                                             (verifica si el server permite
//                                              el embed directo o requiere
//                                              proxy interno)
//
// Cada apertura registra un evento VER_REPORTE con:
//   - Sesion, usuario, IP
//   - CUID del reporte
//   - Todos los parametros ls* del OpenDocument (Almacen, Pais, Fechas)
//     como fila en audit.EventoParametro
//
// Fase actual:
//   Modo A - Embed directo del OpenDocument URL. Si el servidor devuelve
//            X-Frame-Options: DENY se activa el fallback proxy (Modo B).
//   Modo B - Proxy interno: el servidor descarga el HTML/PDF usando el token
//            REST y lo re-sirve desde el propio origen.
//
// Todos los enlaces internos a /Sapbo/... requieren autenticacion Windows.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PortalReportesCrystal.Filters;
using PortalReportesCrystal.Services;

namespace PortalReportesCrystal.Controllers
{
    [Authorize]
    public class SapboController : Controller
    {
        // ==================================================================
        // GET: /Sapbo/Ver?cuid=<CUID>&nombre=...&categoria=...&tipoDoc=...
        //
        // Renderiza una pagina interna con <iframe> apuntando a Contenido().
        // Registra la apertura en auditoria antes de renderizar.
        // ==================================================================
        public ActionResult Ver(
            string cuid,
            string nombre,
            string categoria = null,
            string tipoDoc = null,
            string url = null)
        {
            if (string.IsNullOrWhiteSpace(cuid) && string.IsNullOrWhiteSpace(url))
                return HttpNotFound();

            RegistrarVerReporte(cuid, nombre, categoria, tipoDoc, url);

            // Construimos la URL objetivo de SAP BO (normalizada para embed)
            // y hacemos que el iframe apunte al proxy interno (mismo origen)
            // en lugar de ir directo al CMC. Asi se evitan las restricciones
            // de cookies de terceros y X-Frame-Options.
            string urlSapBo = ConstruirUrlContenido(cuid, tipoDoc, url);
            ViewBag.Cuid = cuid ?? "";
            ViewBag.Nombre = string.IsNullOrWhiteSpace(nombre) ? "Reporte SAP BO" : nombre;
            ViewBag.Categoria = categoria ?? "";
            ViewBag.TipoDoc = tipoDoc ?? "";
            ViewBag.UrlExterna = url ?? "";
            ViewBag.UrlIframe = string.IsNullOrWhiteSpace(urlSapBo)
                ? ""
                : Url.Action("Contenido", "Sapbo", new { url = urlSapBo });

            return View();
        }

        // ==================================================================
        // GET: /Sapbo/Contenido?cuid=<CUID>&tipoDoc=...&url=...
        //
        // Si viene "url", redirige al servidor SAP BO conservando querystring
        // adicional (parametros ls*). Este es el modo simple (embed directo).
        //
        // NOTA: si el servidor SAP BO devuelve X-Frame-Options: DENY se debera
        // reemplazar este redirect por un StreamContent que actue como proxy.
        // Se implementara en la siguiente iteracion segun el resultado de
        // TestIframe().
        // ==================================================================
        public ActionResult Contenido(string cuid, string tipoDoc = null, string url = null, int modo = 0)
        {
            string destino = ConstruirUrlContenido(cuid, tipoDoc, url);
            if (string.IsNullOrWhiteSpace(destino))
                return HttpNotFound();

            // modo=0 (default): redirect directo al CMC.
            //   El navegador se autentica con sus propias cookies/SSO.
            //   Requiere que el usuario ya tenga sesion CMC (o que el CMC
            //   acepte NTLM SSO desde iframes).
            // modo=1: proxy interno (server-side).
            //   El proxy hace login Enterprise y sigue la cadena de auto-submits
            //   del CMC. Utilizado como fallback cuando el redirect no funciona
            //   por bloqueo de cookies de terceros en el navegador.
            if (modo == 1)
            {
                LogProxy("Modo=1 (proxy interno) para " + destino);
                return ProxyRequest(destino);
            }

            LogProxy("Modo=0 (redirect directo) para " + destino);
            return Redirect(destino);
        }

        // ==================================================================
        // GET: /Sapbo/Proxy?u=<url absoluta a SAP BO>
        //
        // Endpoint utilitario para servir sub-recursos (imagenes, JS, CSS,
        // formularios que POSTean al CMC) desde el propio origen del portal.
        // Se usa desde el HTML reescrito por Contenido().
        //
        // La URL se valida contra el host de SAP BO configurado en Web.config
        // para evitar SSRF hacia otros destinos.
        // ==================================================================
        public ActionResult Proxy(string u)
        {
            if (string.IsNullOrWhiteSpace(u) || !EsUrlSapBoAutorizada(u))
                return HttpNotFound();
            return ProxyRequest(u);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProxyPost(string u)
        {
            if (string.IsNullOrWhiteSpace(u) || !EsUrlSapBoAutorizada(u))
                return HttpNotFound();
            return ProxyRequest(u, "POST");
        }

        // ==================================================================
        // GET: /Sapbo/AbrirExterno?url=<url>&nombre=<n>&categoria=<c>&tipoDoc=<t>
        //
        // Endpoint intermedio para el boton "Abrir reporte en SAP BO".
        // Registra el evento APERTURA_EXTERNA en auditoria y luego redirige
        // al servidor SAP BO. Como el <a> tiene target="_blank", el redirect
        // se ejecuta en la nueva pestana.
        //
        // Diferencia clave con Ver(): Ver() registra que el usuario CARGO la
        // pagina intermedia. AbrirExterno() registra que EFECTIVAMENTE hizo
        // clic para irse a SAP BO. Con ambos eventos podemos medir la tasa
        // de conversion "ver reporte -> abrirlo".
        // ==================================================================
        public ActionResult AbrirExterno(
            string url,
            string nombre = null,
            string categoria = null,
            string tipoDoc = null,
            string cuid = null)
        {
            if (string.IsNullOrWhiteSpace(url) || !EsUrlSapBoAutorizada(url))
                return HttpNotFound();

            try
            {
                if (AuditoriaService.Habilitado)
                {
                    var parametros = new System.Collections.Generic.Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(cuid)) parametros["CUID"] = cuid;

                    // Preservar los ls* del OpenDocument (Almacen, Pais, Fechas)
                    var uri = new Uri(url);
                    var qs  = HttpUtility.ParseQueryString(uri.Query);
                    foreach (string key in qs.AllKeys)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!key.StartsWith("ls", StringComparison.OrdinalIgnoreCase)) continue;
                        string nombreParam = key.Length > 3 && (key[2] == 'S' || key[2] == 'N' || key[2] == 'D')
                            ? key.Substring(3)
                            : key.Substring(2);
                        if (string.IsNullOrWhiteSpace(nombreParam)) continue;
                        parametros[nombreParam.ToUpperInvariant()] = qs[key] ?? "";
                    }

                    string servidor = string.Equals(tipoDoc, "CrystalReport", StringComparison.OrdinalIgnoreCase)
                        ? "SAP BO .rpt" : "SAP BO WebI";
                    string tipoReporte = string.Equals(tipoDoc, "CrystalReport", StringComparison.OrdinalIgnoreCase)
                        ? "Sapbo" : "WebI";

                    AuditoriaService.RegistrarEvento(new EventoAuditoria
                    {
                        SesionId = AuditContext.SesionActual(HttpContext),
                        TipoEvento = "DESCARGA_IFRAME",  // reusamos el tipo para "salida a SAP BO"
                        Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                        IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                        RaizId = "sapbo",
                        PathReporte = cuid,
                        NombreReporte = nombre,
                        Categoria = categoria,
                        TipoReporte = tipoReporte,
                        Servidor = servidor,
                        UrlOrigen = url,
                        MensajeError = "APERTURA_EXTERNA: usuario abrio el reporte en pestana nueva",
                        Parametros = parametros.Count > 0 ? parametros : null
                    });
                }
            }
            catch { }

            return Redirect(url);
        }

        // ==================================================================
        // GET: /Sapbo/TestIframe
        //
        // Herramienta de diagnostico solo para admins.
        // Renderiza una pagina que intenta cargar una URL SAP BO en <iframe>
        // y detecta con JavaScript si el iframe termino en blanco (bloqueado
        // por X-Frame-Options) o cargo correctamente.
        // ==================================================================
        [Authorize]
        public ActionResult TestIframe(string url = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                string sapboUrl = ConfigurationManager.AppSettings["SapBo:OpenDocumentUrl"] ?? "";
                if (!string.IsNullOrWhiteSpace(sapboUrl))
                    url = sapboUrl + "?sIDType=CUID&sWindow=Same";
            }
            ViewBag.UrlPrueba = url ?? "";
            return View();
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private string ConstruirUrlContenido(string cuid, string tipoDoc, string url)
        {
            // Si el llamador ya nos paso una URL OpenDocument completa la usamos tal cual,
            // pero normalizando parametros incompatibles con el embed en iframe:
            //   sWindow=New   -> sWindow=Same   (New pide al CMC abrir ventana nueva)
            //   sOutputFormat=H -> sOutputFormat=P
            //     H = viewer HTML del CMC que requiere sesion SAP BO propia y falla
            //         al embeber cross-origin por cookies de terceros bloqueadas.
            //     P = PDF directo. El CMC devuelve un stream PDF que el navegador
            //         embebe nativamente. No depende de la sesion del CMC en el
            //         browser (solo del SSO NTLM/Kerberos por request).
            if (!string.IsNullOrWhiteSpace(url))
            {
                var opts = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                url = System.Text.RegularExpressions.Regex.Replace(url, @"sWindow=New", "sWindow=Same", opts);
                if (System.Text.RegularExpressions.Regex.IsMatch(url, @"sOutputFormat=H(&|$)", opts))
                    url = System.Text.RegularExpressions.Regex.Replace(url, @"sOutputFormat=H", "sOutputFormat=P", opts);
                else if (!System.Text.RegularExpressions.Regex.IsMatch(url, @"sOutputFormat=", opts))
                    url += "&sOutputFormat=P";
                return url;
            }

            string openDoc = (ConfigurationManager.AppSettings["SapBo:OpenDocumentUrl"] ?? "").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(openDoc) || string.IsNullOrWhiteSpace(cuid))
                return null;

            var sb = new StringBuilder();
            sb.Append(openDoc);
            sb.Append("?iDocID=").Append(Uri.EscapeDataString(cuid));
            sb.Append("&sIDType=CUID&sWindow=Same");
            if (string.Equals(tipoDoc, "CrystalReport", StringComparison.OrdinalIgnoreCase))
                sb.Append("&sType=rpt&sOutputFormat=H&sRefresh=Y");

            // Adjuntar todos los ls* que vengan en la querystring del portal
            if (Request != null && Request.QueryString != null)
            {
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    if (!key.StartsWith("ls", StringComparison.OrdinalIgnoreCase)) continue;
                    sb.Append('&').Append(key).Append('=').Append(Uri.EscapeDataString(Request.QueryString[key] ?? ""));
                }
            }
            return sb.ToString();
        }

        private void RegistrarVerReporte(string cuid, string nombre, string categoria, string tipoDoc, string url)
        {
            try
            {
                if (!AuditoriaService.Habilitado) return;

                var parametros = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(cuid)) parametros["CUID"] = cuid;

                // Parsear parametros ls* del OpenDocument (Almacen, Pais, Fechas, etc.)
                if (Request != null && Request.QueryString != null)
                {
                    foreach (string key in Request.QueryString.AllKeys)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!key.StartsWith("ls", StringComparison.OrdinalIgnoreCase)) continue;
                        // ls[SND] + Nombre  ->  Nombre
                        string nombreParam = key.Length > 3 && (key[2] == 'S' || key[2] == 'N' || key[2] == 'D')
                            ? key.Substring(3)
                            : key.Substring(2);
                        if (string.IsNullOrWhiteSpace(nombreParam)) continue;
                        parametros[nombreParam.ToUpperInvariant()] = Request.QueryString[key] ?? "";
                    }
                }

                string servidor = string.Equals(tipoDoc, "CrystalReport", StringComparison.OrdinalIgnoreCase)
                    ? "SAP BO .rpt"
                    : "SAP BO WebI";
                string tipoReporte = string.Equals(tipoDoc, "CrystalReport", StringComparison.OrdinalIgnoreCase)
                    ? "Sapbo"
                    : "WebI";

                AuditoriaService.RegistrarEvento(new EventoAuditoria
                {
                    SesionId = AuditContext.SesionActual(HttpContext),
                    TipoEvento = "VER_REPORTE",
                    Usuario = User != null && User.Identity != null ? User.Identity.Name : null,
                    IpCliente = AuditoriaService.ObtenerIpCliente(System.Web.HttpContext.Current),
                    RaizId = "sapbo",
                    PathReporte = cuid,
                    NombreReporte = nombre,
                    Categoria = categoria,
                    TipoReporte = tipoReporte,
                    Servidor = servidor,
                    UrlOrigen = url,
                    Parametros = parametros.Count > 0 ? parametros : null
                });
            }
            catch { /* nunca reventar el request por auditoria */ }
        }

        // ------------------------------------------------------------------
        // Proxy interno (Modo B)
        // ------------------------------------------------------------------

        // Cookie container por sesion del usuario del portal. Permite que la
        // sesion NTLM/CMC persista entre sub-requests del iframe (imagenes,
        // JS, POST del visor, etc.). Guardar en Session mantiene el estado
        // por-usuario sin filtrar cookies entre usuarios distintos.
        private CookieContainer ObtenerCookieContainer()
        {
            const string key = "__sapbo_proxy_cookies";
            if (Session == null)
                return new CookieContainer();
            var c = Session[key] as CookieContainer;
            if (c == null)
            {
                c = new CookieContainer();
                Session[key] = c;
            }
            return c;
        }

        private static bool _servicePointConfigurado;
        private void ConfigurarServicePoint(Uri destino)
        {
            if (_servicePointConfigurado) return;
            _servicePointConfigurado = true;
            try
            {
                var sp = ServicePointManager.FindServicePoint(destino);
                sp.ConnectionLimit = 20;
                sp.Expect100Continue = false;
            }
            catch { }
        }

        private bool EsUrlSapBoAutorizada(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            Uri destino;
            if (!Uri.TryCreate(url, UriKind.Absolute, out destino)) return false;

            string openDoc = ConfigurationManager.AppSettings["SapBo:OpenDocumentUrl"] ?? "";
            string apiUrl  = ConfigurationManager.AppSettings["SapBo:ApiUrl"] ?? "";

            Uri baseOpenDoc, baseApi;
            bool okOpen = Uri.TryCreate(openDoc, UriKind.Absolute, out baseOpenDoc);
            bool okApi  = Uri.TryCreate(apiUrl,  UriKind.Absolute, out baseApi);

            // Aceptar cualquier ruta bajo el host+puerto del CMC o del API REST
            bool matchOpen = okOpen && destino.Host.Equals(baseOpenDoc.Host, StringComparison.OrdinalIgnoreCase)
                                    && destino.Port == baseOpenDoc.Port;
            bool matchApi  = okApi  && destino.Host.Equals(baseApi.Host, StringComparison.OrdinalIgnoreCase)
                                    && destino.Port == baseApi.Port;

            return matchOpen || matchApi;
        }

        private ActionResult ProxyRequest(string url, string metodoHttp = "GET")
        {
            Uri destino;
            if (!Uri.TryCreate(url, UriKind.Absolute, out destino))
                return HttpNotFound();

            ConfigurarServicePoint(destino);

            var cookies = ObtenerCookieContainer();

            // Asegurar que la sesion SAP BO Enterprise esta activa (login con las
            // credenciales del Web.config si aun no hay cookies validas).
            // NOTA: NO se usa UseDefaultCredentials porque el CMC de SAP BO no
            // autentica con Windows del AppPool. Autenticacion Enterprise via
            // /InfoViewApp/logon.do con las cookies resultantes.
            AsegurarLoginCmc(cookies, destino);

            var req = (HttpWebRequest)WebRequest.Create(destino);
            req.Method = metodoHttp;
            req.CookieContainer = cookies;
            req.AllowAutoRedirect = false;   // manejamos manualmente para reescribir Location
            req.UserAgent = Request != null ? (Request.UserAgent ?? "PortalReportesCrystal/1.0") : "PortalReportesCrystal/1.0";
            req.Timeout = 60000;
            req.ReadWriteTimeout = 60000;
            req.KeepAlive = true;

            // Si la URL solicita PDF (sOutputFormat=P), forzar Accept: application/pdf
            // para que el CMC devuelva el binario PDF y no el visor HTML del BILaunchPad.
            bool esOpenDocPdf =
                (destino.AbsolutePath.IndexOf("openDocument.jsp", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 destino.AbsolutePath.IndexOf("custom.jsp", StringComparison.OrdinalIgnoreCase) >= 0) &&
                Regex.IsMatch(destino.Query, @"sOutputFormat=P(&|$)", RegexOptions.IgnoreCase);

            if (esOpenDocPdf)
            {
                req.Accept = "application/pdf";
                LogProxy("Forzando Accept: application/pdf para openDocument.jsp con sOutputFormat=P");
            }
            else if (Request != null && !string.IsNullOrEmpty(Request.Headers["Accept"]))
            {
                req.Accept = Request.Headers["Accept"];
            }

            if (Request != null)
            {
                string lang = Request.Headers["Accept-Language"];
                if (!string.IsNullOrEmpty(lang)) req.Headers["Accept-Language"] = lang;
            }

            // Copiar cuerpo si es POST
            if (metodoHttp == "POST" && Request != null && Request.InputStream != null && Request.InputStream.Length > 0)
            {
                req.ContentType = Request.ContentType ?? "application/x-www-form-urlencoded";
                Request.InputStream.Position = 0;
                using (var reqStream = req.GetRequestStream())
                {
                    Request.InputStream.CopyTo(reqStream);
                }
            }

            LogProxy("Proxy " + metodoHttp + " " + destino);

            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException wex) when (wex.Response != null)
            {
                resp = (HttpWebResponse)wex.Response;
                LogProxy("Proxy WebException capturada, se sirve respuesta del CMC: HTTP " + (int)resp.StatusCode);
            }
            catch (Exception ex)
            {
                LogProxy("Proxy EXC " + ex.GetType().Name + ": " + ex.Message);
                Response.StatusCode = 502;
                return Content("Proxy SAP BO fallo: " + ex.Message, "text/plain");
            }

            LogProxy("Proxy respuesta HTTP " + (int)resp.StatusCode + " ContentType=" + (resp.ContentType ?? "?"));

            using (resp)
            {
                // Manejar redirects sin salir del proxy
                if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode < 400)
                {
                    string location = resp.Headers["Location"];
                    if (!string.IsNullOrEmpty(location))
                    {
                        Uri loc;
                        if (Uri.TryCreate(destino, location, out loc) && EsUrlSapBoAutorizada(loc.AbsoluteUri))
                            return Redirect(BuildProxyUrl(loc.AbsoluteUri));
                    }
                }

                string contentType = resp.ContentType ?? "application/octet-stream";
                Response.StatusCode = (int)resp.StatusCode;

                // Reenviar cabeceras seguras (no todas — evitar Transfer-Encoding, Server, etc.)
                foreach (string h in new[] { "Cache-Control", "Content-Language", "Expires", "Last-Modified" })
                {
                    string v = resp.Headers[h];
                    if (!string.IsNullOrEmpty(v)) Response.Headers[h] = v;
                }

                using (var respStream = resp.GetResponseStream())
                {
                    if (respStream == null)
                        return new EmptyResult();

                    // Si es HTML/XML/JS/CSS: leer, reescribir URLs, devolver texto
                    bool esHtml   = contentType.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0
                                 || contentType.IndexOf("xhtml", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool esCssJs  = contentType.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) >= 0
                                 || contentType.IndexOf("/css", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (esHtml || esCssJs)
                    {
                        string charset = "UTF-8";
                        int idx = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);
                        if (idx > 0)
                        {
                            charset = contentType.Substring(idx + 8).Trim();
                            int semi = charset.IndexOf(';');
                            if (semi > 0) charset = charset.Substring(0, semi).Trim();
                        }
                        Encoding enc;
                        try { enc = Encoding.GetEncoding(charset); }
                        catch { enc = Encoding.UTF8; }

                        string body;
                        using (var sr = new StreamReader(respStream, enc))
                            body = sr.ReadToEnd();

                        // DEBUG: guardar el body original y el reescrito para inspeccion
                        try
                        {
                            string dbgDir = Server != null ? Server.MapPath("~/App_Data") : null;
                            if (!string.IsNullOrEmpty(dbgDir) && Directory.Exists(dbgDir))
                            {
                                string tag = esHtml ? "html" : "cssjs";
                                System.IO.File.WriteAllText(Path.Combine(dbgDir, "proxy_last_" + tag + "_original.txt"), body, Encoding.UTF8);
                            }
                        }
                        catch { }

                        LogProxy("Body ANTES len=" + body.Length + " (primeros 200): " + body.Substring(0, Math.Min(200, body.Length)).Replace('\n', ' ').Replace('\r', ' '));

                        // === Interceptor de auto-submit del CMC ===
                        // OpenDocument devuelve una pagina con <form> + JavaScript isApplication()
                        // que hace .submit() automatico. La URL del form es relativa y el
                        // navegador la resuelve contra localhost, no contra sapbo. Aqui
                        // detectamos ese patron y hacemos el POST server-side para saltar
                        // el intermediario JavaScript.
                        if (esHtml && body.IndexOf("isApplication()") >= 0 &&
                            body.IndexOf("document.forms[0].submit()") >= 0)
                        {
                            LogProxy("Detectado auto-submit form del CMC. Ejecutando POST server-side...");
                            var finalResp = EjecutarAutoSubmit(body, destino, cookies);
                            if (finalResp != null)
                            {
                                resp.Close();
                                return finalResp;
                            }
                            LogProxy("EjecutarAutoSubmit devolvio null; se sirve el HTML original.");
                        }

                        int longAntes = body.Length;
                        if (esHtml)
                            body = ReescribirHtml(body, destino);
                        else
                            body = ReescribirCssJs(body, destino);

                        LogProxy("Body DESPUES len=" + body.Length + " (delta=" + (body.Length - longAntes) + ")");

                        try
                        {
                            string dbgDir = Server != null ? Server.MapPath("~/App_Data") : null;
                            if (!string.IsNullOrEmpty(dbgDir) && Directory.Exists(dbgDir))
                            {
                                string tag = esHtml ? "html" : "cssjs";
                                System.IO.File.WriteAllText(Path.Combine(dbgDir, "proxy_last_" + tag + "_reescrito.txt"), body, Encoding.UTF8);
                            }
                        }
                        catch { }

                        Response.ContentType = contentType;
                        return Content(body, contentType, enc);
                    }

                    // Binario / PDF / imagenes: streaming pass-through
                    Response.ContentType = contentType;
                    respStream.CopyTo(Response.OutputStream);
                    return new EmptyResult();
                }
            }
        }

        // Reescribe atributos href/src/action que apuntan al servidor SAP BO
        // para que pasen por /Sapbo/Proxy?u=<url>. Cubre absolutas y relativas.
        private string ReescribirHtml(string html, Uri baseUri)
        {
            // 1) URLs absolutas al servidor SAP BO -> /Sapbo/Proxy?u=...
            html = Regex.Replace(
                html,
                @"(?<attr>href|src|action|background)\s*=\s*(?<q>[""'])(?<url>https?://[^""']+)\k<q>",
                m =>
                {
                    string origUrl = m.Groups["url"].Value;
                    if (!EsUrlSapBoAutorizada(origUrl)) return m.Value;
                    return m.Groups["attr"].Value + "=" + m.Groups["q"].Value + BuildProxyUrl(origUrl) + m.Groups["q"].Value;
                },
                RegexOptions.IgnoreCase);

            // 2) URLs relativas empezando con / (rutas absolutas del server) -> /Sapbo/Proxy?u=<baseHost>/...
            html = Regex.Replace(
                html,
                @"(?<attr>href|src|action|background)\s*=\s*(?<q>[""'])(?<url>/[^""']*)\k<q>",
                m =>
                {
                    string origUrl = m.Groups["url"].Value;
                    // Evitar reescribir anclas hash-only
                    if (origUrl.StartsWith("//")) return m.Value;
                    var abs = new Uri(baseUri, origUrl).AbsoluteUri;
                    return m.Groups["attr"].Value + "=" + m.Groups["q"].Value + BuildProxyUrl(abs) + m.Groups["q"].Value;
                },
                RegexOptions.IgnoreCase);

            // 3) window.open / location a URLs SAP BO — bloquear la apertura de ventanas
            //    externas (no vale la pena reescribir; el visor a veces las usa para logout).
            //    Se puede afinar despues segun errores en runtime.
            return html;
        }

        private string ReescribirCssJs(string body, Uri baseUri)
        {
            // url(...) en CSS
            body = Regex.Replace(
                body,
                @"url\(\s*['""]?(?<url>[^)'""]+)['""]?\s*\)",
                m =>
                {
                    string origUrl = m.Groups["url"].Value.Trim();
                    if (string.IsNullOrEmpty(origUrl) || origUrl.StartsWith("data:")) return m.Value;
                    string abs;
                    try { abs = new Uri(baseUri, origUrl).AbsoluteUri; }
                    catch { return m.Value; }
                    if (!EsUrlSapBoAutorizada(abs)) return m.Value;
                    return "url('" + BuildProxyUrl(abs) + "')";
                },
                RegexOptions.IgnoreCase);
            return body;
        }

        private string BuildProxyUrl(string absoluteUrl)
        {
            return Url.Action("Proxy", "Sapbo", new { u = absoluteUrl });
        }

        // ------------------------------------------------------------------
        // Interceptor de auto-submit del CMC
        // ------------------------------------------------------------------
        // OpenDocument devuelve un HTML con este patron:
        //   <form method="POST" action="../../OpenDocument/.../openDocument.jsp">
        //     <input type="hidden" name="iDocID" value="..." />
        //     <input type="hidden" name="token" value="..." />
        //     ...
        //   </form>
        //   <script>document.forms[0].userParamsList.value="..."; document.forms[0].submit();</script>
        //
        // El submit lo intenta el navegador con URL relativa que en nuestro
        // proxy queda apuntando a localhost. Aqui armamos el POST desde el
        // server usando la URL absoluta y devolvemos su respuesta como si
        // fuera la respuesta original del proxy.
        private ActionResult EjecutarAutoSubmit(string html, Uri baseUri, CookieContainer cookies)
        {
            try
            {
                // 1. Extraer action del form (viene HTML-encoded con &#x2f;)
                var actionM = Regex.Match(html,
                    @"<form[^>]*\baction\s*=\s*[""']([^""']+)[""'][^>]*>",
                    RegexOptions.IgnoreCase);
                if (!actionM.Success) return null;

                string action = HttpUtility.HtmlDecode(actionM.Groups[1].Value);
                Uri actionUri;
                try { actionUri = new Uri(baseUri, action); } catch { return null; }
                if (!EsUrlSapBoAutorizada(actionUri.AbsoluteUri)) return null;

                // 2. Extraer todos los inputs hidden del form
                var kvps = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>();
                var inputs = Regex.Matches(html,
                    @"<input[^>]*\btype\s*=\s*[""']hidden[""'][^>]*>",
                    RegexOptions.IgnoreCase);
                foreach (Match inp in inputs)
                {
                    string s = inp.Value;
                    var nM = Regex.Match(s, @"\bname\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                    var vM = Regex.Match(s, @"\bvalue\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                    if (nM.Success)
                    {
                        string name  = HttpUtility.HtmlDecode(nM.Groups[1].Value);
                        string value = vM.Success ? HttpUtility.HtmlDecode(vM.Groups[1].Value) : "";
                        kvps.Add(new System.Collections.Generic.KeyValuePair<string, string>(name, value));
                    }
                }

                // 3. Recuperar los valores que el JS setea en runtime:
                //    isApplication, appKind, userParamsList
                var isAppM  = Regex.Match(html, @"isApplication\.value\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                var appKM   = Regex.Match(html, @"appKind\.value\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
                var userPmM = Regex.Match(html, @"userParamsList\.value\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);

                void SetOrAdd(string name, string value)
                {
                    int idx = kvps.FindIndex(k => k.Key == name);
                    if (idx >= 0) kvps[idx] = new System.Collections.Generic.KeyValuePair<string, string>(name, value);
                    else          kvps.Add(new System.Collections.Generic.KeyValuePair<string, string>(name, value));
                }
                if (isAppM.Success)  SetOrAdd("isApplication", HttpUtility.HtmlDecode(isAppM.Groups[1].Value));
                if (appKM.Success)   SetOrAdd("appKind",       HttpUtility.HtmlDecode(appKM.Groups[1].Value));
                if (userPmM.Success) SetOrAdd("userParamsList", HttpUtility.HtmlDecode(userPmM.Groups[1].Value));

                // 4. Serializar como application/x-www-form-urlencoded
                var sb = new StringBuilder();
                foreach (var kv in kvps)
                {
                    if (sb.Length > 0) sb.Append('&');
                    sb.Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value ?? ""));
                }
                byte[] postBody = Encoding.UTF8.GetBytes(sb.ToString());

                LogProxy("AutoSubmit POST " + actionUri + " campos=" + kvps.Count + " lenBody=" + postBody.Length);

                // 5. POST server-side siguiendo redirects, con las mismas cookies
                var req = (HttpWebRequest)WebRequest.Create(actionUri);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = postBody.Length;
                req.CookieContainer = cookies;
                req.AllowAutoRedirect = false;
                req.UserAgent = Request != null ? (Request.UserAgent ?? "PortalReportesCrystal/1.0") : "PortalReportesCrystal/1.0";
                req.Timeout = 90000;
                req.ReadWriteTimeout = 90000;
                req.KeepAlive = true;
                req.Accept = "text/html,application/xhtml+xml,application/xml,image/*,*/*;q=0.8";

                // Si el POST apunta a openDocument.jsp y los campos incluyen
                // sOutputFormat=P, forzar Accept: application/pdf.
                bool esOpenDocPost =
                    actionUri.AbsolutePath.IndexOf("openDocument.jsp", StringComparison.OrdinalIgnoreCase) >= 0
                    && kvps.Exists(k => k.Key == "sOutputFormat" && k.Value == "P");
                if (esOpenDocPost)
                {
                    req.Accept = "application/pdf";
                    LogProxy("Forzando Accept: application/pdf en AutoSubmit POST (sOutputFormat=P)");
                }

                using (var s = req.GetRequestStream()) s.Write(postBody, 0, postBody.Length);

                HttpWebResponse finalResp;
                try { finalResp = (HttpWebResponse)req.GetResponse(); }
                catch (WebException wex) when (wex.Response != null) { finalResp = (HttpWebResponse)wex.Response; }

                using (finalResp)
                {
                    LogProxy("AutoSubmit respuesta HTTP " + (int)finalResp.StatusCode + " ContentType=" + (finalResp.ContentType ?? "?"));

                    // Si hay redirect via Location header, lo seguimos con el proxy
                    if ((int)finalResp.StatusCode >= 300 && (int)finalResp.StatusCode < 400)
                    {
                        string location = finalResp.Headers["Location"];
                        if (!string.IsNullOrEmpty(location))
                        {
                            Uri loc;
                            if (Uri.TryCreate(actionUri, location, out loc) && EsUrlSapBoAutorizada(loc.AbsoluteUri))
                            {
                                LogProxy("AutoSubmit redirect Location=" + loc);
                                return ProxyRequest(loc.AbsoluteUri, "GET");
                            }
                        }
                    }

                    string ct = finalResp.ContentType ?? "application/octet-stream";
                    bool esHtmlFinal = ct.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (esHtmlFinal)
                    {
                        // Leer y analizar por si contiene MAS auto-submit / redirect JS
                        string finalBody;
                        using (var rs = finalResp.GetResponseStream())
                        using (var srF = new StreamReader(rs, Encoding.UTF8))
                            finalBody = srF.ReadToEnd();

                        try
                        {
                            string dbgDir = Server != null ? Server.MapPath("~/App_Data") : null;
                            if (!string.IsNullOrEmpty(dbgDir))
                                System.IO.File.WriteAllText(Path.Combine(dbgDir, "proxy_last_autosubmit.txt"), finalBody, Encoding.UTF8);
                        }
                        catch { }

                        LogProxy("AutoSubmit body HTML len=" + finalBody.Length + " (200): " +
                            finalBody.Substring(0, Math.Min(200, finalBody.Length)).Replace('\n',' ').Replace('\r',' '));

                        // Detectar mas patrones de navegacion server-side:
                        // 1) Otro form auto-submit
                        if (finalBody.IndexOf("document.forms[0].submit()", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            LogProxy("AutoSubmit anidado detectado, ejecutando recursivamente...");
                            var recur = EjecutarAutoSubmit(finalBody, actionUri, cookies);
                            if (recur != null) return recur;
                        }

                        // 2) Redirect JavaScript (location.href, location.replace, window.location)
                        Uri redir = ExtraerRedirectJs(finalBody, actionUri);
                        if (redir != null && EsUrlSapBoAutorizada(redir.AbsoluteUri))
                        {
                            LogProxy("AutoSubmit redirect JS detectado: " + redir);
                            return ProxyRequest(redir.AbsoluteUri, "GET");
                        }

                        // 3) <meta http-equiv="refresh" content="0;url=...">
                        var metaM = Regex.Match(finalBody,
                            @"<meta\s+http-equiv\s*=\s*[""']refresh[""'][^>]*content\s*=\s*[""'][^""']*url\s*=\s*([^""']+)[""']",
                            RegexOptions.IgnoreCase);
                        if (metaM.Success)
                        {
                            string mUrl = HttpUtility.HtmlDecode(metaM.Groups[1].Value);
                            Uri mLoc;
                            if (Uri.TryCreate(actionUri, mUrl, out mLoc) && EsUrlSapBoAutorizada(mLoc.AbsoluteUri))
                            {
                                LogProxy("AutoSubmit meta-refresh: " + mLoc);
                                return ProxyRequest(mLoc.AbsoluteUri, "GET");
                            }
                        }

                        // Ningun patron mas: reescribir URLs y devolver el HTML
                        finalBody = ReescribirHtml(finalBody, actionUri);
                        Response.StatusCode = (int)finalResp.StatusCode;
                        return Content(finalBody, ct, Encoding.UTF8);
                    }

                    // No es HTML (PDF, imagen, etc.) - pass-through
                    Response.StatusCode = (int)finalResp.StatusCode;
                    Response.ContentType = ct;
                    using (var rs = finalResp.GetResponseStream())
                    {
                        if (rs != null)
                            rs.CopyTo(Response.OutputStream);
                    }
                    return new EmptyResult();
                }
            }
            catch (Exception ex)
            {
                LogProxy("EjecutarAutoSubmit EXC " + ex.GetType().Name + ": " + ex.Message);
                return null;
            }
        }

        // Detecta redirecciones JavaScript comunes en HTML del CMC:
        //   location.href = "..."
        //   window.location = "..."
        //   document.location = "..."
        //   location.replace("...")
        //   top.location.href = "..."
        // Devuelve la URI absoluta si el patron es reconocible; null si no.
        private Uri ExtraerRedirectJs(string html, Uri baseUri)
        {
            if (string.IsNullOrEmpty(html)) return null;

            var patterns = new[]
            {
                @"(?:top\.|window\.|document\.)?location(?:\.href)?\s*=\s*[""']([^""']+)[""']",
                @"location\.replace\s*\(\s*[""']([^""']+)[""']\s*\)",
            };

            foreach (var pat in patterns)
            {
                var m = Regex.Match(html, pat, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string raw = HttpUtility.HtmlDecode(m.Groups[1].Value);
                    Uri result;
                    if (Uri.TryCreate(baseUri, raw, out result))
                        return result;
                }
            }
            return null;
        }

        // Log dedicado del proxy (App_Data\proxy_sapbo.log). Ligero y no bloqueante.
        private void LogProxy(string mensaje)
        {
            try
            {
                string dir = Server != null ? Server.MapPath("~/App_Data") : null;
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
                string ruta = Path.Combine(dir, "proxy_sapbo.log");
                string linea = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}",
                    DateTime.Now, mensaje, Environment.NewLine);
                System.IO.File.AppendAllText(ruta, linea, Encoding.UTF8);
            }
            catch { }
        }

        // ------------------------------------------------------------------
        // Login Enterprise contra el CMC (InfoView)
        // ------------------------------------------------------------------
        // Se llama antes de cada request del proxy. Si Session ya tiene la
        // marca de login exitoso, hace nada. En caso contrario POSTea las
        // credenciales de Web.config al endpoint SapBo:LogonPath para poblar
        // el CookieContainer con las cookies de sesion CMC.
        //
        // Manejo de fallo: se registra el intento en el log de SAP BO y se
        // deja seguir. El proxy respondera con la pagina que devuelva el CMC
        // (habitualmente la propia pagina de logon).
        private void AsegurarLoginCmc(CookieContainer cookies, Uri targetUri)
        {
            const string flagKey = "__sapbo_cmc_login_ok";
            try
            {
                if (Session != null && Session[flagKey] is bool b && b) return;
            }
            catch { }

            string usuario  = ConfigurationManager.AppSettings["SapBo:Usuario"]  ?? "";
            string password = ConfigurationManager.AppSettings["SapBo:Password"] ?? "";
            string tipoAuth = ConfigurationManager.AppSettings["SapBo:TipoAuth"] ?? "secEnterprise";
            string cmsName  = ConfigurationManager.AppSettings["SapBo:CmsName"]  ?? "";
            string logonPth = ConfigurationManager.AppSettings["SapBo:LogonPath"] ?? "/InfoViewApp/logon.do";

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
                return;

            Uri logonUri;
            try
            {
                var baseHost = new Uri(targetUri.GetLeftPart(UriPartial.Authority));
                logonUri = new Uri(baseHost, logonPth);
            }
            catch { return; }

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(cmsName))
                sb.Append("cms=").Append(Uri.EscapeDataString(cmsName)).Append('&');
            sb.Append("username=").Append(Uri.EscapeDataString(usuario));
            sb.Append("&password=").Append(Uri.EscapeDataString(password));
            sb.Append("&authType=").Append(Uri.EscapeDataString(tipoAuth));
            // Nombres alternativos que algunos endpoints XI 3.1 esperan:
            sb.Append("&authenticationType=").Append(Uri.EscapeDataString(tipoAuth));

            byte[] body = Encoding.UTF8.GetBytes(sb.ToString());

            var req = (HttpWebRequest)WebRequest.Create(logonUri);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = body.Length;
            req.CookieContainer = cookies;
            req.AllowAutoRedirect = true;  // seguimos redirects para asentar cookies
            req.UserAgent = "PortalReportesCrystal/1.0";
            req.Timeout = 30000;
            req.ReadWriteTimeout = 30000;
            req.Accept = "text/html,application/xhtml+xml";

            LogProxy("Login intento: POST " + logonUri + " (user=" + usuario + " cms=" + cmsName + " auth=" + tipoAuth + ")");

            try
            {
                using (var s = req.GetRequestStream())
                    s.Write(body, 0, body.Length);

                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    int cookiesCount = 0;
                    try { cookiesCount = cookies.GetCookies(logonUri).Count; } catch { }

                    LogProxy("Login OK HTTP " + (int)resp.StatusCode + " ResponseUri=" + resp.ResponseUri + " cookies=" + cookiesCount);

                    // Aceptamos cualquier 2xx / 3xx como "login realizado". El CMC
                    // suele redirigir a la landing de InfoView tras logon exitoso.
                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 400)
                    {
                        if (Session != null) Session[flagKey] = true;
                    }
                }
            }
            catch (WebException wex)
            {
                string detalle = wex.Message;
                int httpStatus = 0;
                try
                {
                    if (wex.Response != null)
                    {
                        httpStatus = (int)((HttpWebResponse)wex.Response).StatusCode;
                        using (var er = wex.Response.GetResponseStream())
                        using (var sr = new StreamReader(er, Encoding.UTF8))
                        {
                            string b = sr.ReadToEnd();
                            detalle += " Body(" + b.Length + "): " + b.Substring(0, Math.Min(300, b.Length));
                        }
                    }
                }
                catch { }
                LogProxy("Login FAIL HTTP " + httpStatus + " " + detalle);
            }
            catch (Exception ex)
            {
                LogProxy("Login EXC " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
